using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class InstallmentPlanService : IInstallmentPlanService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public InstallmentPlanService(ApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Task<InstallmentPlan?> GetByInvoiceIdAsync(long invoiceId, CancellationToken cancellationToken = default) =>
        _db.InstallmentPlans
            .Include(p => p.Schedules)
            .FirstOrDefaultAsync(p => p.InvoiceId == invoiceId, cancellationToken);

    public Task<InstallmentPlan?> GetByIdAsync(long installmentPlanId, CancellationToken cancellationToken = default) =>
        _db.InstallmentPlans
            .Include(p => p.Invoice)
            .Include(p => p.Schedules)
            .FirstOrDefaultAsync(p => p.InstallmentPlanId == installmentPlanId, cancellationToken);

    public async Task<InstallmentPlan> CreateAsync(InstallmentPlan plan, CancellationToken cancellationToken = default)
    {
        if (plan.NumberOfInstallments <= 0)
        {
            throw new BusinessRuleException("Number of installments must be greater than zero.");
        }

        if (plan.StartDate > plan.EndDate)
        {
            throw new BusinessRuleException("Start date must not occur after end date.");
        }

        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == plan.InvoiceId && !i.IsDeleted, cancellationToken)
            ?? throw new BusinessRuleException("Invoice not found.");

        if (!_currentUser.CanAccessBranch(invoice.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this invoice's branch.");
        }

        if (invoice.InvoiceStatus != InvoiceStatus.Active)
        {
            throw new BusinessRuleException("An installment plan can only be created for an active invoice.");
        }

        var existingPlan = await _db.InstallmentPlans.AnyAsync(p => p.InvoiceId == plan.InvoiceId, cancellationToken);
        if (existingPlan)
        {
            throw new BusinessRuleException("This invoice already has an installment plan.");
        }

        var eligibleAmount = invoice.OutstandingAmount - plan.DownPayment;
        if (eligibleAmount <= 0)
        {
            throw new BusinessRuleException("Down payment must be less than the invoice's outstanding balance.");
        }

        plan.TotalInstallmentAmount = eligibleAmount;
        plan.Status = InstallmentPlanStatus.PendingApproval;
        plan.CreatedAtUtc = DateTime.UtcNow;
        plan.Schedules = GenerateSchedule(plan);

        _db.InstallmentPlans.Add(plan);
        await _db.SaveChangesAsync(cancellationToken);
        return plan;
    }

    public async Task ApproveAsync(long installmentPlanId, string approvedByUserId, CancellationToken cancellationToken = default)
    {
        var plan = await _db.InstallmentPlans.FirstOrDefaultAsync(p => p.InstallmentPlanId == installmentPlanId, cancellationToken)
            ?? throw new BusinessRuleException("Installment plan not found.");

        plan.Status = InstallmentPlanStatus.Active;
        plan.ApprovedByUserId = approvedByUserId;
        plan.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(long installmentPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _db.InstallmentPlans.FirstOrDefaultAsync(p => p.InstallmentPlanId == installmentPlanId, cancellationToken)
            ?? throw new BusinessRuleException("Installment plan not found.");

        plan.Status = InstallmentPlanStatus.Cancelled;
        plan.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Splits TotalInstallmentAmount evenly across NumberOfInstallments, folding any
    /// rounding remainder into the final installment so the schedule sums exactly to the
    /// plan total (spec 13.5: "Handle decimal remainder safely").
    /// </summary>
    private static List<InstallmentSchedule> GenerateSchedule(InstallmentPlan plan)
    {
        var baseAmount = Math.Round(plan.TotalInstallmentAmount / plan.NumberOfInstallments, 2, MidpointRounding.ToZero);
        var schedules = new List<InstallmentSchedule>();
        var runningTotal = 0m;

        var interval = plan.Frequency switch
        {
            InstallmentFrequency.Weekly => TimeSpan.FromDays(7),
            InstallmentFrequency.BiWeekly => TimeSpan.FromDays(14),
            InstallmentFrequency.Monthly => TimeSpan.FromDays(30),
            InstallmentFrequency.Quarterly => TimeSpan.FromDays(90),
            _ => TimeSpan.FromDays(30)
        };

        for (var i = 1; i <= plan.NumberOfInstallments; i++)
        {
            var isLast = i == plan.NumberOfInstallments;
            var amount = isLast ? plan.TotalInstallmentAmount - runningTotal : baseAmount;
            runningTotal += amount;

            schedules.Add(new InstallmentSchedule
            {
                InstallmentNumber = i,
                DueDate = plan.StartDate.Add(TimeSpan.FromTicks(interval.Ticks * i)),
                AmountDue = amount,
                AmountPaid = 0m,
                Status = InstallmentStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        return schedules;
    }
}
