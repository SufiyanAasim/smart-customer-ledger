using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class ReconciliationService : IReconciliationService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IAuditLogService _auditLog;

    public ReconciliationService(ApplicationDbContext db, ICurrentUserContext currentUser, IAuditLogService auditLog)
    {
        _db = db;
        _currentUser = currentUser;
        _auditLog = auditLog;
    }

    public async Task<ReconciliationReport> ReconcileCustomerAccountAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken)
            ?? throw new BusinessRuleException("Customer not found.");

        if (!_currentUser.CanAccessBranch(customer.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this customer's branch.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(a => a.CustomerId == customerId, cancellationToken)
            ?? throw new BusinessRuleException("Customer account not found.");

        // Recalculated purely from source-of-truth rows — never from the account's own
        // (possibly drifted) TotalBilled/TotalPaid columns.
        var recalculatedBilled = await _db.Invoices
            .Where(i => i.CustomerId == customerId && !i.IsDeleted && i.InvoiceStatus == InvoiceStatus.Active)
            .SumAsync(i => i.TotalAmount, cancellationToken);

        var recalculatedPaid = await _db.Payments
            .Where(p => p.CustomerId == customerId && p.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(p => p.Amount, cancellationToken);

        var report = new ReconciliationReport
        {
            CustomerId = customerId,
            CustomerName = customer.FullName,
            PreviousTotalBilled = account.TotalBilled,
            RecalculatedTotalBilled = recalculatedBilled,
            PreviousTotalPaid = account.TotalPaid,
            RecalculatedTotalPaid = recalculatedPaid,
            PreviousCurrentBalance = account.CurrentBalance,
            RecalculatedCurrentBalance = recalculatedBilled - recalculatedPaid
        };

        if (report.HadMismatch)
        {
            account.TotalBilled = recalculatedBilled;
            account.TotalPaid = recalculatedPaid;
            account.CurrentBalance = recalculatedBilled - recalculatedPaid;
            account.UpdatedAtUtc = DateTime.UtcNow;
            account.ConcurrencyVersion++;
            await _db.SaveChangesAsync(cancellationToken);

            await _auditLog.WriteAsync(new AuditLog
            {
                UserId = _currentUser.UserId,
                BranchId = customer.BranchId,
                TableName = "CustomerAccounts",
                RecordId = account.CustomerAccountId.ToString(),
                ActionType = "Reconcile",
                OldValuesJson = System.Text.Json.JsonSerializer.Serialize(new { report.PreviousTotalBilled, report.PreviousTotalPaid, report.PreviousCurrentBalance }),
                NewValuesJson = System.Text.Json.JsonSerializer.Serialize(new { report.RecalculatedTotalBilled, report.RecalculatedTotalPaid, report.RecalculatedCurrentBalance }),
                CreatedAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return report;
    }

    public async Task<IReadOnlyList<ReconciliationReport>> ReconcileBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.CanAccessBranch(branchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this branch.");
        }

        var customerIds = await _db.Customers
            .Where(c => c.BranchId == branchId && !c.IsDeleted)
            .Select(c => c.CustomerId)
            .ToListAsync(cancellationToken);

        var reports = new List<ReconciliationReport>();
        foreach (var customerId in customerIds)
        {
            reports.Add(await ReconcileCustomerAccountAsync(customerId, cancellationToken));
        }

        return reports;
    }
}
