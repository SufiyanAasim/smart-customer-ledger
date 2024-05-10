using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Interfaces;

public interface ICustomerAccountService
{
    Task<CustomerAccount?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);

    /// <summary>Creates the one-and-only account for a newly registered customer.</summary>
    Task<CustomerAccount> CreateForCustomerAsync(int customerId, decimal creditLimit, CancellationToken cancellationToken = default);

    /// <summary>Updates only the fields an operator may edit directly (CreditLimit, AccountStatus) — never the calculated totals.</summary>
    Task UpdateCreditLimitAsync(int customerAccountId, decimal newCreditLimit, CancellationToken cancellationToken = default);
}
