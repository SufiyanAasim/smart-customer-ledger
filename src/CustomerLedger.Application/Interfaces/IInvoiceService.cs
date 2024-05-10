using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Index-level invoice foundation: header + line items with recalculated totals while the
/// invoice is still Draft. The full transactional posting workflow (payments, account
/// balance sync) is Balance-release scope — see docs/releases/v2.0.0-Balance.md.
/// </summary>
public interface IInvoiceService
{
    Task<PagedResult<Invoice>> GetPagedAsync(int? branchId, int? customerId, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdAsync(long invoiceId, CancellationToken cancellationToken = default);
    Task<Invoice> CreateDraftAsync(Invoice invoice, IReadOnlyList<InvoiceItem> items, CancellationToken cancellationToken = default);
    Task AddItemAsync(long invoiceId, InvoiceItem item, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(long invoiceId, long invoiceItemId, CancellationToken cancellationToken = default);
    Task ActivateAsync(long invoiceId, CancellationToken cancellationToken = default);
    Task CancelAsync(long invoiceId, CancellationToken cancellationToken = default);
}
