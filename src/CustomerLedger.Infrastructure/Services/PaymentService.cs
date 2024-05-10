using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

/// <summary>
/// Full transactional payment workflow (Balance release): posting and reversing a payment
/// both lock the invoice row with SELECT ... FOR UPDATE for the duration of the
/// transaction, so two concurrent payment requests against the same invoice cannot both
/// read the same OutstandingAmount and jointly overpay it — the second request blocks
/// until the first commits, then re-validates against the now-current balance.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IAuditLogService _auditLog;

    public PaymentService(ApplicationDbContext db, ICurrentUserContext currentUser, IAuditLogService auditLog)
    {
        _db = db;
        _currentUser = currentUser;
        _auditLog = auditLog;
    }

    public async Task<PagedResult<Payment>> GetPagedAsync(int? branchId, long? invoiceId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Payments.AsNoTracking().AsQueryable();

        if (!_currentUser.IsAdministrator)
        {
            query = query.Where(p => p.BranchId == _currentUser.BranchId);
        }
        else if (branchId.HasValue)
        {
            query = query.Where(p => p.BranchId == branchId.Value);
        }

        if (invoiceId.HasValue)
        {
            query = query.Where(p => p.InvoiceId == invoiceId.Value);
        }

        query = query.OrderByDescending(p => p.PaymentDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(p => p.Invoice)
            .Include(p => p.Customer)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Payment>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Payment?> GetByIdAsync(long paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _db.Payments
            .Include(p => p.Invoice)
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId, cancellationToken);

        if (payment is not null && !_currentUser.CanAccessBranch(payment.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this payment's branch.");
        }

        return payment;
    }

    public async Task<Payment> RecordPaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment.Amount <= 0)
        {
            throw new BusinessRuleException("Payment amount must be greater than zero.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        // Row lock: any other transaction trying to SELECT ... FOR UPDATE or write this
        // same invoice blocks here until we commit or roll back — this is what prevents
        // two concurrent payments from both reading the same OutstandingAmount.
        var invoice = await _db.Invoices
            .FromSqlInterpolated($"SELECT * FROM Invoices WHERE InvoiceId = {payment.InvoiceId} AND IsDeleted = 0 FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BusinessRuleException("Invoice not found.");

        if (!_currentUser.CanAccessBranch(invoice.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this invoice's branch.");
        }

        if (invoice.InvoiceStatus != InvoiceStatus.Active)
        {
            throw new BusinessRuleException("Payments can only be posted against an active invoice.");
        }

        if (invoice.OutstandingAmount <= 0)
        {
            throw new BusinessRuleException("This invoice is already fully paid.");
        }

        if (payment.Amount > invoice.OutstandingAmount)
        {
            throw new BusinessRuleException("Payment amount cannot exceed the invoice's outstanding balance.");
        }

        payment.CustomerId = invoice.CustomerId;
        payment.BranchId = invoice.BranchId;
        payment.PaymentDate = payment.PaymentDate == default ? DateTime.UtcNow : payment.PaymentDate;
        payment.PaymentStatus = PaymentStatus.Completed;
        payment.ReceivedByUserId = _currentUser.UserId ?? throw new BusinessRuleException("Authenticated user required.");
        payment.CreatedAtUtc = DateTime.UtcNow;

        _db.Payments.Add(payment);

        invoice.PaidAmount += payment.Amount;
        invoice.OutstandingAmount = invoice.TotalAmount - invoice.PaidAmount;
        invoice.PaymentStatus = invoice.OutstandingAmount <= 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        invoice.ConcurrencyVersion++;

        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(a => a.CustomerId == invoice.CustomerId, cancellationToken)
            ?? throw new BusinessRuleException("Customer account not found.");
        account.TotalPaid += payment.Amount;
        account.CurrentBalance = account.TotalBilled - account.TotalPaid;
        account.UpdatedAtUtc = DateTime.UtcNow;
        account.ConcurrencyVersion++;

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(new AuditLog
        {
            UserId = _currentUser.UserId,
            BranchId = payment.BranchId,
            TableName = "Payments",
            RecordId = payment.PaymentId.ToString(),
            ActionType = "Create",
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return payment;
    }

    public async Task<Payment> ReverseAsync(long paymentId, string reversalReason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reversalReason))
        {
            throw new BusinessRuleException("A reversal reason is required.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var original = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId, cancellationToken)
            ?? throw new BusinessRuleException("Payment not found.");

        if (!_currentUser.CanAccessBranch(original.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this payment's branch.");
        }

        if (original.PaymentStatus != PaymentStatus.Completed)
        {
            throw new BusinessRuleException("Only a completed payment can be reversed.");
        }

        var alreadyReversed = await _db.Payments.AnyAsync(p => p.ReversedPaymentId == original.PaymentId, cancellationToken);
        if (alreadyReversed)
        {
            throw new BusinessRuleException("This payment has already been reversed.");
        }

        // Same row-lock discipline as RecordPaymentAsync — a reversal mutates the same
        // invoice totals a concurrent new payment might be posting against.
        var invoice = await _db.Invoices
            .FromSqlInterpolated($"SELECT * FROM Invoices WHERE InvoiceId = {original.InvoiceId} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BusinessRuleException("Invoice not found.");

        var originalStatusBeforeReversal = original.PaymentStatus;
        original.PaymentStatus = PaymentStatus.Reversed;
        original.ReversalReason = reversalReason;
        original.UpdatedAtUtc = DateTime.UtcNow;

        var reversal = new Payment
        {
            InvoiceId = original.InvoiceId,
            CustomerId = original.CustomerId,
            BranchId = original.BranchId,
            PaymentNumber = $"{original.PaymentNumber}-REV",
            PaymentDate = DateTime.UtcNow,
            Amount = original.Amount,
            PaymentMethod = original.PaymentMethod,
            PaymentStatus = PaymentStatus.Reversed,
            ReceivedByUserId = _currentUser.UserId ?? throw new BusinessRuleException("Authenticated user required."),
            ReversedPaymentId = original.PaymentId,
            ReversalReason = reversalReason,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Payments.Add(reversal);

        invoice.PaidAmount -= original.Amount;
        invoice.OutstandingAmount = invoice.TotalAmount - invoice.PaidAmount;
        invoice.PaymentStatus = invoice.PaidAmount <= 0 ? PaymentStatus.Unpaid : PaymentStatus.PartiallyPaid;
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        invoice.ConcurrencyVersion++;

        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(a => a.CustomerId == invoice.CustomerId, cancellationToken)
            ?? throw new BusinessRuleException("Customer account not found.");
        account.TotalPaid -= original.Amount;
        account.CurrentBalance = account.TotalBilled - account.TotalPaid;
        account.UpdatedAtUtc = DateTime.UtcNow;
        account.ConcurrencyVersion++;

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(new AuditLog
        {
            UserId = _currentUser.UserId,
            BranchId = original.BranchId,
            TableName = "Payments",
            RecordId = original.PaymentId.ToString(),
            ActionType = "Reverse",
            OldValuesJson = System.Text.Json.JsonSerializer.Serialize(new { PaymentStatus = originalStatusBeforeReversal }),
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return reversal;
    }
}
