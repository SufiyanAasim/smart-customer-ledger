using CustomerLedger.Application.Exceptions;
using CustomerLedger.DatabaseTests;
using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using CustomerLedger.Infrastructure.Services;
using CustomerLedger.IntegrationTests.Fakes;

namespace CustomerLedger.IntegrationTests.Services;

public class InvoiceServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public InvoiceServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(ApplicationDbContext Db, Branch Branch, Customer Customer, string UserId)> SeedBranchAndCustomerAsync(ApplicationDbContext db)
    {
        var branch = new Branch { BranchCode = $"INV-{Guid.NewGuid():N}"[..12], Name = "Invoice Test Branch", PhoneNumber = "0", Address = "n/a", City = "n/a", CreatedAtUtc = DateTime.UtcNow };
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var user = new Domain.Entities.ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"user-{Guid.NewGuid():N}@test.local",
            Email = $"user-{Guid.NewGuid():N}@test.local",
            FullName = "Test Cashier",
            EmployeeCode = $"EMP-{Guid.NewGuid():N}"[..15],
            BranchId = branch.BranchId,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);

        var customer = new Customer
        {
            BranchId = branch.BranchId,
            CustomerCode = $"CUST-{Guid.NewGuid():N}"[..15],
            FullName = "Invoice Test Customer",
            PhoneNumber = "0",
            Address = "n/a",
            City = "n/a",
            RegistrationDate = DateTime.UtcNow,
            Status = CustomerStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        return (db, branch, customer, user.Id);
    }

    [MySqlAvailableFact]
    public async Task CreateDraftAsync_CalculatesTotalsFromItems()
    {
        await using var db = _fixture.CreateContext();
        var (_, branch, customer, userId) = await SeedBranchAndCustomerAsync(db);
        var currentUser = FakeCurrentUserContext.ForBranch(branch.BranchId);
        currentUser.UserId = userId;
        var sut = new InvoiceService(db, currentUser, new AuditLogService(db));

        var invoice = await sut.CreateDraftAsync(
            new Invoice { CustomerId = customer.CustomerId, BranchId = branch.BranchId, InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..15], InvoiceDate = DateTime.UtcNow },
            new List<InvoiceItem> { new() { Description = "Item", Quantity = 2, UnitPrice = 100m, DiscountAmount = 10m, TaxAmount = 5m } });

        Assert.Equal(195m, invoice.TotalAmount); // (2*100) - 10 + 5
        Assert.Equal(195m, invoice.OutstandingAmount);
        Assert.Equal(InvoiceStatus.Draft, invoice.InvoiceStatus);
    }

    [MySqlAvailableFact]
    public async Task ActivateAsync_WithNoItems_ThrowsBusinessRuleException()
    {
        await using var db = _fixture.CreateContext();
        var (_, branch, customer, userId) = await SeedBranchAndCustomerAsync(db);
        var currentUser = FakeCurrentUserContext.ForBranch(branch.BranchId);
        currentUser.UserId = userId;
        var sut = new InvoiceService(db, currentUser, new AuditLogService(db));

        var invoice = await sut.CreateDraftAsync(
            new Invoice { CustomerId = customer.CustomerId, BranchId = branch.BranchId, InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..15], InvoiceDate = DateTime.UtcNow },
            new List<InvoiceItem>());

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.ActivateAsync(invoice.InvoiceId));
    }

    [MySqlAvailableFact]
    public async Task CreateDraftAsync_ForInactiveCustomer_ThrowsBusinessRuleException()
    {
        await using var db = _fixture.CreateContext();
        var (_, branch, customer, userId) = await SeedBranchAndCustomerAsync(db);
        customer.Status = CustomerStatus.Inactive;
        await db.SaveChangesAsync();

        var currentUser = FakeCurrentUserContext.ForBranch(branch.BranchId);
        currentUser.UserId = userId;
        var sut = new InvoiceService(db, currentUser, new AuditLogService(db));

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.CreateDraftAsync(
            new Invoice { CustomerId = customer.CustomerId, BranchId = branch.BranchId, InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..15], InvoiceDate = DateTime.UtcNow },
            new List<InvoiceItem> { new() { Description = "Item", Quantity = 1, UnitPrice = 10m } }));
    }
}
