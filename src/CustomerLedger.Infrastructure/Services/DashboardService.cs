using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public DashboardService(ApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummary> GetSummaryAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        int? scopeBranchId = _currentUser.IsAdministrator ? branchId : _currentUser.BranchId;

        var customers = _db.Customers.AsNoTracking().Where(c => !c.IsDeleted && c.Status == CustomerStatus.Active);
        var invoices = _db.Invoices.AsNoTracking().Where(i => !i.IsDeleted && i.InvoiceStatus == InvoiceStatus.Active);
        var installments = _db.InstallmentSchedules.AsNoTracking()
            .Where(s => s.Status == InstallmentStatus.Pending && s.DueDate < DateTime.UtcNow);
        var interactions = _db.CustomerInteractions.AsNoTracking().Where(ci => ci.Status == InteractionStatus.Open);
        var today = DateTime.UtcNow.Date;
        var payments = _db.Payments.AsNoTracking()
            .Where(p => p.PaymentStatus == PaymentStatus.Completed && p.PaymentDate.Date == today);

        if (scopeBranchId.HasValue)
        {
            customers = customers.Where(c => c.BranchId == scopeBranchId.Value);
            invoices = invoices.Where(i => i.BranchId == scopeBranchId.Value);
            interactions = interactions.Where(ci => ci.BranchId == scopeBranchId.Value);
            payments = payments.Where(p => p.BranchId == scopeBranchId.Value);
            installments = installments.Where(s => s.InstallmentPlan.Invoice.BranchId == scopeBranchId.Value);
        }

        return new DashboardSummary
        {
            TotalActiveCustomers = await customers.CountAsync(cancellationToken),
            TotalActiveInvoices = await invoices.CountAsync(cancellationToken),
            TotalOutstandingBalance = await invoices.SumAsync(i => i.OutstandingAmount, cancellationToken),
            OverdueInstallmentCount = await installments.CountAsync(cancellationToken),
            OpenInteractionCount = await interactions.CountAsync(cancellationToken),
            TodaysCollectedAmount = await payments.SumAsync(p => p.Amount, cancellationToken)
        };
    }
}
