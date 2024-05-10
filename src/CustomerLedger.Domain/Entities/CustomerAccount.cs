using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Domain.Entities;

/// <summary>
/// One-to-one financial ledger for a customer. Totals (TotalBilled/TotalPaid/CurrentBalance)
/// are maintained exclusively by transactional application services (see the Balance
/// release) — never edited directly through a form. ConcurrencyVersion guards against
/// lost updates from concurrent payment postings.
/// </summary>
public class CustomerAccount
{
    public int CustomerAccountId { get; set; }
    public int CustomerId { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal TotalPaid { get; set; }
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public uint ConcurrencyVersion { get; set; }

    public Customer Customer { get; set; } = null!;
}
