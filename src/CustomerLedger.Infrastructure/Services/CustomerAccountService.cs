using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class CustomerAccountService : ICustomerAccountService
{
    private readonly ApplicationDbContext _db;

    public CustomerAccountService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<CustomerAccount?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default) =>
        _db.CustomerAccounts.FirstOrDefaultAsync(a => a.CustomerId == customerId, cancellationToken);

    public async Task<CustomerAccount> CreateForCustomerAsync(int customerId, decimal creditLimit, CancellationToken cancellationToken = default)
    {
        if (creditLimit < 0)
        {
            throw new BusinessRuleException("Credit limit cannot be negative.");
        }

        var exists = await _db.CustomerAccounts.AnyAsync(a => a.CustomerId == customerId, cancellationToken);
        if (exists)
        {
            throw new BusinessRuleException("This customer already has a financial account.");
        }

        var account = new CustomerAccount
        {
            CustomerId = customerId,
            CreditLimit = creditLimit,
            CurrentBalance = 0m,
            TotalBilled = 0m,
            TotalPaid = 0m,
            AccountStatus = AccountStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.CustomerAccounts.Add(account);
        await _db.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task UpdateCreditLimitAsync(int customerAccountId, decimal newCreditLimit, CancellationToken cancellationToken = default)
    {
        if (newCreditLimit < 0)
        {
            throw new BusinessRuleException("Credit limit cannot be negative.");
        }

        var account = await _db.CustomerAccounts.FirstOrDefaultAsync(a => a.CustomerAccountId == customerAccountId, cancellationToken)
            ?? throw new BusinessRuleException("Customer account not found.");

        account.CreditLimit = newCreditLimit;
        account.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
