using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Results;
using CustomerLedger.Application.Services;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IAuditLogService _auditLog;

    public InvoiceService(ApplicationDbContext db, ICurrentUserContext currentUser, IAuditLogService auditLog)
    {
        _db = db;
        _currentUser = currentUser;
        _auditLog = auditLog;
    }

    public async Task<PagedResult<Invoice>> GetPagedAsync(int? branchId, int? customerId, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Invoices.AsNoTracking().Where(i => !i.IsDeleted);

        if (!_currentUser.IsAdministrator)
        {
            query = query.Where(i => i.BranchId == _currentUser.BranchId);
        }
        else if (branchId.HasValue)
        {
            query = query.Where(i => i.BranchId == branchId.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(i => i.InvoiceStatus == parsedStatus);
        }

        query = query.OrderByDescending(i => i.InvoiceDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(i => i.Customer)
            .Include(i => i.Branch)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Invoice>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Invoice?> GetByIdAsync(long invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Branch)
            .Include(i => i.InvoiceItems)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && !i.IsDeleted, cancellationToken);

        if (invoice is not null && !_currentUser.CanAccessBranch(invoice.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this invoice's branch.");
        }

        return invoice;
    }

    public async Task<Invoice> CreateDraftAsync(Invoice invoice, IReadOnlyList<InvoiceItem> items, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.CanAccessBranch(invoice.BranchId))
        {
            throw new BranchAccessDeniedException("You cannot create an invoice for a different branch.");
        }

        var customer = await _db.Customers.FirstOrDefaultAsync(
            c => c.CustomerId == invoice.CustomerId && !c.IsDeleted, cancellationToken)
            ?? throw new BusinessRuleException("Customer not found.");

        if (customer.BranchId != invoice.BranchId)
        {
            throw new BusinessRuleException("Invoice must belong to the customer's branch.");
        }

        if (customer.Status != CustomerStatus.Active)
        {
            throw new BusinessRuleException("Cannot create an invoice for an inactive customer.");
        }

        var numberExists = await _db.Invoices.AnyAsync(i => i.InvoiceNumber == invoice.InvoiceNumber, cancellationToken);
        if (numberExists)
        {
            throw new BusinessRuleException($"Invoice number '{invoice.InvoiceNumber}' is already in use.");
        }

        invoice.InvoiceDate = invoice.InvoiceDate == default ? DateTime.UtcNow : invoice.InvoiceDate;
        invoice.InvoiceStatus = InvoiceStatus.Draft;
        invoice.PaymentStatus = PaymentStatus.Unpaid;
        invoice.PaidAmount = 0m;
        invoice.CreatedByUserId = _currentUser.UserId ?? throw new BusinessRuleException("Authenticated user required.");
        invoice.CreatedAtUtc = DateTime.UtcNow;
        invoice.InvoiceItems = items.ToList();

        InvoiceCalculationService.RecalculateInvoiceTotals(invoice);

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(new AuditLog
        {
            UserId = _currentUser.UserId,
            BranchId = invoice.BranchId,
            TableName = "Invoices",
            RecordId = invoice.InvoiceId.ToString(),
            ActionType = "Create",
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return invoice;
    }

    public async Task AddItemAsync(long invoiceId, InvoiceItem item, CancellationToken cancellationToken = default)
    {
        var invoice = await LoadEditableDraftAsync(invoiceId, cancellationToken);

        item.CreatedAtUtc = DateTime.UtcNow;
        invoice.InvoiceItems.Add(item);
        InvoiceCalculationService.RecalculateInvoiceTotals(invoice);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(long invoiceId, long invoiceItemId, CancellationToken cancellationToken = default)
    {
        var invoice = await LoadEditableDraftAsync(invoiceId, cancellationToken);

        var item = invoice.InvoiceItems.FirstOrDefault(i => i.InvoiceItemId == invoiceItemId)
            ?? throw new BusinessRuleException("Invoice item not found.");

        invoice.InvoiceItems.Remove(item);
        _db.InvoiceItems.Remove(item);
        InvoiceCalculationService.RecalculateInvoiceTotals(invoice);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateAsync(long invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await LoadEditableDraftAsync(invoiceId, cancellationToken);

        if (!invoice.InvoiceItems.Any())
        {
            throw new BusinessRuleException("An invoice must have at least one item before it can be activated.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        invoice.InvoiceStatus = InvoiceStatus.Active;
        invoice.DueDate ??= invoice.InvoiceDate.AddDays(30);
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        invoice.ConcurrencyVersion++;

        // Only an Active invoice represents real customer debt — Draft invoices can still
        // be freely edited/cancelled with zero financial consequence, so TotalBilled is
        // synced here rather than at Create (spec 13.1 step 14-15, applied at the point the
        // invoice becomes billable rather than at header creation, since Index's Draft
        // workflow didn't exist when that step order was originally specified).
        var account = await LoadAccountForUpdateAsync(invoice.CustomerId, cancellationToken);
        account.TotalBilled += invoice.TotalAmount;
        account.CurrentBalance = account.TotalBilled - account.TotalPaid;
        account.UpdatedAtUtc = DateTime.UtcNow;
        account.ConcurrencyVersion++;

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(new AuditLog
        {
            UserId = _currentUser.UserId,
            BranchId = invoice.BranchId,
            TableName = "Invoices",
            RecordId = invoice.InvoiceId.ToString(),
            ActionType = "Activate",
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task CancelAsync(long invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && !i.IsDeleted, cancellationToken)
            ?? throw new BusinessRuleException("Invoice not found.");

        if (!_currentUser.CanAccessBranch(invoice.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this invoice's branch.");
        }

        if (invoice.Payments.Any(p => p.PaymentStatus == PaymentStatus.Completed))
        {
            throw new BusinessRuleException("Invoices with recorded payments cannot be cancelled — reverse the payments first.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var wasActive = invoice.InvoiceStatus == InvoiceStatus.Active;

        invoice.InvoiceStatus = InvoiceStatus.Cancelled;
        invoice.UpdatedAtUtc = DateTime.UtcNow;
        invoice.ConcurrencyVersion++;

        if (wasActive)
        {
            // Undo the TotalBilled contribution this invoice made at Activate time —
            // a cancelled invoice must not count toward the customer's outstanding debt.
            var account = await LoadAccountForUpdateAsync(invoice.CustomerId, cancellationToken);
            account.TotalBilled -= invoice.TotalAmount;
            account.CurrentBalance = account.TotalBilled - account.TotalPaid;
            account.UpdatedAtUtc = DateTime.UtcNow;
            account.ConcurrencyVersion++;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(new AuditLog
        {
            UserId = _currentUser.UserId,
            BranchId = invoice.BranchId,
            TableName = "Invoices",
            RecordId = invoice.InvoiceId.ToString(),
            ActionType = "Cancel",
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<CustomerAccount> LoadAccountForUpdateAsync(int customerId, CancellationToken cancellationToken)
    {
        return await _db.CustomerAccounts.FirstOrDefaultAsync(a => a.CustomerId == customerId, cancellationToken)
            ?? throw new BusinessRuleException("Customer account not found.");
    }

    private async Task<Invoice> LoadEditableDraftAsync(long invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && !i.IsDeleted, cancellationToken)
            ?? throw new BusinessRuleException("Invoice not found.");

        if (!_currentUser.CanAccessBranch(invoice.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this invoice's branch.");
        }

        if (invoice.InvoiceStatus != InvoiceStatus.Draft)
        {
            throw new BusinessRuleException("Invoice items can only be changed while the invoice is in Draft status.");
        }

        return invoice;
    }
}
