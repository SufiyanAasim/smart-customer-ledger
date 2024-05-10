using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class InstallmentScheduleService : IInstallmentScheduleService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IPaymentService _paymentService;

    public InstallmentScheduleService(ApplicationDbContext db, ICurrentUserContext currentUser, IPaymentService paymentService)
    {
        _db = db;
        _currentUser = currentUser;
        _paymentService = paymentService;
    }

    public async Task<Payment> PayInstallmentAsync(long installmentScheduleId, decimal amount, PaymentMethod paymentMethod, string? transactionReference, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleException("Payment amount must be greater than zero.");
        }

        var schedule = await _db.InstallmentSchedules
            .Include(s => s.InstallmentPlan)
            .ThenInclude(p => p.Invoice)
            .FirstOrDefaultAsync(s => s.InstallmentScheduleId == installmentScheduleId, cancellationToken)
            ?? throw new BusinessRuleException("Installment schedule not found.");

        if (schedule.InstallmentPlan.Status != InstallmentPlanStatus.Active)
        {
            throw new BusinessRuleException("Installments can only be paid on an active (approved) plan.");
        }

        if (schedule.Status is InstallmentStatus.Cancelled)
        {
            throw new BusinessRuleException("This installment has been cancelled.");
        }

        var remaining = schedule.AmountDue - schedule.AmountPaid;
        if (amount > remaining)
        {
            throw new BusinessRuleException("Payment amount cannot exceed this installment's remaining balance.");
        }

        // Delegates to PaymentService so the invoice's PaidAmount/OutstandingAmount and the
        // CustomerAccount's TotalPaid/CurrentBalance are updated through the one code path
        // that owns that arithmetic (spec: avoid duplicate financial calculation logic).
        var payment = await _paymentService.RecordPaymentAsync(new Payment
        {
            InvoiceId = schedule.InstallmentPlan.InvoiceId,
            PaymentNumber = $"INST-{schedule.InstallmentScheduleId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Amount = amount,
            PaymentMethod = paymentMethod,
            TransactionReference = transactionReference,
            Notes = $"Installment #{schedule.InstallmentNumber} of plan {schedule.InstallmentPlanId}"
        }, cancellationToken);

        schedule.AmountPaid += amount;
        schedule.UpdatedAtUtc = DateTime.UtcNow;
        if (schedule.AmountPaid >= schedule.AmountDue)
        {
            schedule.Status = InstallmentStatus.Paid;
            schedule.PaidDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var plan = schedule.InstallmentPlan;
        var allPaid = await _db.InstallmentSchedules
            .Where(s => s.InstallmentPlanId == plan.InstallmentPlanId)
            .AllAsync(s => s.Status == InstallmentStatus.Paid || s.Status == InstallmentStatus.Cancelled, cancellationToken);
        if (allPaid)
        {
            plan.Status = InstallmentPlanStatus.Completed;
            plan.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return payment;
    }

    public async Task<int> MarkOverdueInstallmentsAsync(CancellationToken cancellationToken = default)
    {
        var overdueRows = await _db.InstallmentSchedules
            .Where(s => s.Status == InstallmentStatus.Pending && s.DueDate < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var row in overdueRows)
        {
            row.Status = InstallmentStatus.Overdue;
            row.UpdatedAtUtc = DateTime.UtcNow;
        }

        if (overdueRows.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return overdueRows.Count;
    }
}
