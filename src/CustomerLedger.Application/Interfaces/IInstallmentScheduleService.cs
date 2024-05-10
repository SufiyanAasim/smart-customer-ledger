using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Applies a payment against one installment schedule row. Delegates the actual money
/// movement to IPaymentService.RecordPaymentAsync (single source of truth for invoice and
/// customer-account balance sync), then marks the schedule row itself paid/overdue.
/// </summary>
public interface IInstallmentScheduleService
{
    Task<Payment> PayInstallmentAsync(long installmentScheduleId, decimal amount, PaymentMethod paymentMethod, string? transactionReference, CancellationToken cancellationToken = default);

    /// <summary>Transitions every still-Pending row whose DueDate has passed to Overdue. Time does not advance a trigger by itself — see spec 12 — so this is invoked by a scheduled mechanism, not automatically.</summary>
    Task<int> MarkOverdueInstallmentsAsync(CancellationToken cancellationToken = default);
}
