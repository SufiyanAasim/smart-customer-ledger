using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace CustomerLedger.DatabaseTests;

public class ReferentialIntegrityTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ReferentialIntegrityTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [MySqlAvailableFact]
    public async Task Customer_WithNonExistentBranch_IsRejectedByForeignKey()
    {
        await using var context = _fixture.CreateContext();

        context.Customers.Add(new Customer
        {
            BranchId = 999_999,
            CustomerCode = "FK-TEST-001",
            FullName = "Referential Integrity Test",
            PhoneNumber = "0000000000",
            Address = "n/a",
            City = "n/a",
            RegistrationDate = DateTime.UtcNow,
            Status = CustomerStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [MySqlAvailableFact]
    public async Task Branch_CannotBeDeleted_WhilePhysicallyReferenced()
    {
        await using var context = _fixture.CreateContext();

        var branch = new Branch
        {
            BranchCode = "REF-TEST",
            Name = "Referential Test Branch",
            PhoneNumber = "0000000000",
            Address = "n/a",
            City = "n/a",
            CreatedAtUtc = DateTime.UtcNow
        };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        context.Customers.Add(new Customer
        {
            BranchId = branch.BranchId,
            CustomerCode = "REF-TEST-CUST",
            FullName = "Referential Test Customer",
            PhoneNumber = "0000000000",
            Address = "n/a",
            City = "n/a",
            RegistrationDate = DateTime.UtcNow,
            Status = CustomerStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        context.Branches.Remove(branch);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
