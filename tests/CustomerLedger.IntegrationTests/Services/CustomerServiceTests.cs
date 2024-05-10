using CustomerLedger.Application.Exceptions;
using CustomerLedger.DatabaseTests;
using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Services;
using CustomerLedger.IntegrationTests.Fakes;

namespace CustomerLedger.IntegrationTests.Services;

public class CustomerServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public CustomerServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(CustomerLedger.Infrastructure.Data.Contexts.ApplicationDbContext Db, Branch BranchA, Branch BranchB)> SeedTwoBranchesAsync()
    {
        var db = _fixture.CreateContext();

        var branchA = new Branch { BranchCode = $"CST-A-{Guid.NewGuid():N}"[..12], Name = "Branch A", PhoneNumber = "0", Address = "n/a", City = "n/a", CreatedAtUtc = DateTime.UtcNow };
        var branchB = new Branch { BranchCode = $"CST-B-{Guid.NewGuid():N}"[..12], Name = "Branch B", PhoneNumber = "0", Address = "n/a", City = "n/a", CreatedAtUtc = DateTime.UtcNow };
        db.Branches.AddRange(branchA, branchB);
        await db.SaveChangesAsync();

        return (db, branchA, branchB);
    }

    [MySqlAvailableFact]
    public async Task CreateAsync_RegistersCustomerAndCreatesLinkedAccount()
    {
        var (db, branchA, _) = await SeedTwoBranchesAsync();
        var currentUser = FakeCurrentUserContext.ForBranch(branchA.BranchId);
        var auditLog = new AuditLogService(db);
        var accountService = new CustomerAccountService(db);
        var sut = new CustomerService(db, currentUser, accountService, auditLog);

        var customer = await sut.CreateAsync(new Customer
        {
            BranchId = branchA.BranchId,
            CustomerCode = $"CODE-{Guid.NewGuid():N}"[..15],
            FullName = "Test Customer",
            PhoneNumber = "0300-0000000",
            Address = "n/a",
            City = "n/a"
        }, initialCreditLimit: 10000m);

        var account = await accountService.GetByCustomerIdAsync(customer.CustomerId);
        Assert.NotNull(account);
        Assert.Equal(10000m, account!.CreditLimit);
    }

    [MySqlAvailableFact]
    public async Task CreateAsync_DuplicateCustomerCode_ThrowsBusinessRuleException()
    {
        var (db, branchA, _) = await SeedTwoBranchesAsync();
        var currentUser = FakeCurrentUserContext.ForBranch(branchA.BranchId);
        var accountService = new CustomerAccountService(db);
        var sut = new CustomerService(db, currentUser, accountService, new AuditLogService(db));

        var code = $"DUP-{Guid.NewGuid():N}"[..15];
        await sut.CreateAsync(new Customer { BranchId = branchA.BranchId, CustomerCode = code, FullName = "First", PhoneNumber = "0", Address = "n/a", City = "n/a" }, 0);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            sut.CreateAsync(new Customer { BranchId = branchA.BranchId, CustomerCode = code, FullName = "Second", PhoneNumber = "0", Address = "n/a", City = "n/a" }, 0));
    }

    [MySqlAvailableFact]
    public async Task CreateAsync_ForDifferentBranchThanCurrentUser_ThrowsBranchAccessDeniedException()
    {
        var (db, branchA, branchB) = await SeedTwoBranchesAsync();
        var currentUser = FakeCurrentUserContext.ForBranch(branchA.BranchId); // not an administrator, not branchB
        var accountService = new CustomerAccountService(db);
        var sut = new CustomerService(db, currentUser, accountService, new AuditLogService(db));

        await Assert.ThrowsAsync<BranchAccessDeniedException>(() =>
            sut.CreateAsync(new Customer
            {
                BranchId = branchB.BranchId,
                CustomerCode = $"XBR-{Guid.NewGuid():N}"[..15],
                FullName = "Cross Branch Attempt",
                PhoneNumber = "0",
                Address = "n/a",
                City = "n/a"
            }, 0));
    }

    [MySqlAvailableFact]
    public async Task GetByIdAsync_FromAnotherBranch_ThrowsBranchAccessDeniedException()
    {
        var (db, branchA, branchB) = await SeedTwoBranchesAsync();
        var adminUser = FakeCurrentUserContext.ForAdministrator();
        var accountService = new CustomerAccountService(db);
        var adminService = new CustomerService(db, adminUser, accountService, new AuditLogService(db));

        var customer = await adminService.CreateAsync(new Customer
        {
            BranchId = branchB.BranchId,
            CustomerCode = $"OTH-{Guid.NewGuid():N}"[..15],
            FullName = "Branch B Customer",
            PhoneNumber = "0",
            Address = "n/a",
            City = "n/a"
        }, 0);

        var staffInBranchA = FakeCurrentUserContext.ForBranch(branchA.BranchId);
        var staffService = new CustomerService(db, staffInBranchA, accountService, new AuditLogService(db));

        await Assert.ThrowsAsync<BranchAccessDeniedException>(() => staffService.GetByIdAsync(customer.CustomerId));
    }
}
