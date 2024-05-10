using CustomerLedger.DatabaseTests;
using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Services;
using CustomerLedger.IntegrationTests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.IntegrationTests.Services;

public class ReconciliationServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ReconciliationServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [MySqlAvailableFact]
    public async Task ReconcileCustomerAccountAsync_CorrectsDriftedTotals()
    {
        await using var db = _fixture.CreateContext();

        var branch = new Branch { BranchCode = $"RCN-{Guid.NewGuid():N}"[..12], Name = "Reconciliation Branch", PhoneNumber = "0", Address = "n/a", City = "n/a", CreatedAtUtc = DateTime.UtcNow };
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var customer = new Customer { BranchId = branch.BranchId, CustomerCode = $"C-{Guid.NewGuid():N}"[..15], FullName = "Drifted Customer", PhoneNumber = "0", Address = "n/a", City = "n/a", RegistrationDate = DateTime.UtcNow, Status = CustomerStatus.Active, CreatedAtUtc = DateTime.UtcNow };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        // Deliberately wrong stored totals — as if a bug or manual edit corrupted them.
        db.CustomerAccounts.Add(new CustomerAccount { CustomerId = customer.CustomerId, CreditLimit = 0, TotalBilled = 99999m, TotalPaid = 12345m, CurrentBalance = 87654m, AccountStatus = AccountStatus.Active, CreatedAtUtc = DateTime.UtcNow });

        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = $"u-{Guid.NewGuid():N}@t.local", Email = $"u-{Guid.NewGuid():N}@t.local", FullName = "Cashier", EmployeeCode = $"E-{Guid.NewGuid():N}"[..15], CreatedAtUtc = DateTime.UtcNow };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.Invoices.Add(new Invoice
        {
            CustomerId = customer.CustomerId,
            BranchId = branch.BranchId,
            InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..15],
            InvoiceDate = DateTime.UtcNow,
            Subtotal = 500m,
            TotalAmount = 500m,
            OutstandingAmount = 500m,
            PaymentStatus = PaymentStatus.Unpaid,
            InvoiceStatus = InvoiceStatus.Active,
            CreatedByUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var currentUser = FakeCurrentUserContext.ForAdministrator();
        var sut = new ReconciliationService(db, currentUser, new AuditLogService(db));

        var report = await sut.ReconcileCustomerAccountAsync(customer.CustomerId);

        Assert.True(report.HadMismatch);
        Assert.Equal(500m, report.RecalculatedTotalBilled);
        Assert.Equal(0m, report.RecalculatedTotalPaid);
        Assert.Equal(500m, report.RecalculatedCurrentBalance);

        var account = await db.CustomerAccounts.FirstAsync(a => a.CustomerId == customer.CustomerId);
        Assert.Equal(500m, account.TotalBilled);
        Assert.Equal(0m, account.TotalPaid);
    }

    [MySqlAvailableFact]
    public async Task ReconcileCustomerAccountAsync_WhenAlreadyCorrect_ReportsNoMismatch()
    {
        await using var db = _fixture.CreateContext();

        var branch = new Branch { BranchCode = $"OK-{Guid.NewGuid():N}"[..12], Name = "Correct Branch", PhoneNumber = "0", Address = "n/a", City = "n/a", CreatedAtUtc = DateTime.UtcNow };
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var customer = new Customer { BranchId = branch.BranchId, CustomerCode = $"C-{Guid.NewGuid():N}"[..15], FullName = "Correct Customer", PhoneNumber = "0", Address = "n/a", City = "n/a", RegistrationDate = DateTime.UtcNow, Status = CustomerStatus.Active, CreatedAtUtc = DateTime.UtcNow };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        db.CustomerAccounts.Add(new CustomerAccount { CustomerId = customer.CustomerId, CreditLimit = 0, TotalBilled = 0, TotalPaid = 0, CurrentBalance = 0, AccountStatus = AccountStatus.Active, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var currentUser = FakeCurrentUserContext.ForAdministrator();
        var sut = new ReconciliationService(db, currentUser, new AuditLogService(db));

        var report = await sut.ReconcileCustomerAccountAsync(customer.CustomerId);

        Assert.False(report.HadMismatch);
    }
}
