using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.DatabaseTests;

public class UniqueConstraintTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UniqueConstraintTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [MySqlAvailableFact]
    public async Task Branch_DuplicateBranchCode_IsRejected()
    {
        await using var context = _fixture.CreateContext();

        context.Branches.Add(new Branch
        {
            BranchCode = "DUP-TEST",
            Name = "First Branch",
            PhoneNumber = "0000000000",
            Address = "n/a",
            City = "n/a",
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        context.Branches.Add(new Branch
        {
            BranchCode = "DUP-TEST",
            Name = "Second Branch — Should Fail",
            PhoneNumber = "0000000000",
            Address = "n/a",
            City = "n/a",
            CreatedAtUtc = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [MySqlAvailableFact]
    public async Task CustomerAccount_SecondAccountForSameCustomer_IsRejected()
    {
        await using var context = _fixture.CreateContext();

        var branch = new Branch
        {
            BranchCode = "ACC-TEST",
            Name = "Account Test Branch",
            PhoneNumber = "0000000000",
            Address = "n/a",
            City = "n/a",
            CreatedAtUtc = DateTime.UtcNow
        };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var customer = new Customer
        {
            BranchId = branch.BranchId,
            CustomerCode = "ACC-TEST-CUST",
            FullName = "Account Test Customer",
            PhoneNumber = "0000000000",
            Address = "n/a",
            City = "n/a",
            RegistrationDate = DateTime.UtcNow,
            Status = Domain.Enums.CustomerStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        context.CustomerAccounts.Add(new CustomerAccount
        {
            CustomerId = customer.CustomerId,
            CreditLimit = 0,
            AccountStatus = Domain.Enums.AccountStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        context.CustomerAccounts.Add(new CustomerAccount
        {
            CustomerId = customer.CustomerId,
            CreditLimit = 0,
            AccountStatus = Domain.Enums.AccountStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
