using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Full transactional payment workflow (Balance release): posting and reversing payments,
/// each keeping the invoice and customer account balances in sync within one transaction.
/// </summary>
public interface IPaymentService
{
    Task<PagedResult<Payment>> GetPagedAsync(int? branchId, long? invoiceId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdAsync(long paymentId, CancellationToken cancellationToken = default);
    Task<Payment> RecordPaymentAsync(Payment payment, CancellationToken cancellationToken = default);

    /// <summary>Reverses a completed payment via a linked second row — the original is never deleted, only marked Reversed.</summary>
    Task<Payment> ReverseAsync(long paymentId, string reversalReason, CancellationToken cancellationToken = default);
}
