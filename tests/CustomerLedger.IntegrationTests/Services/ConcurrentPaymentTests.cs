using CustomerLedger.Application.Exceptions;
using CustomerLedger.DatabaseTests;
using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Services;
using CustomerLedger.IntegrationTests.Fakes;

namespace CustomerLedger.IntegrationTests.Services;

/// <summary>
/// Proves Isolation: two payments, each individually valid but jointly overpaying the
/// invoice, are submitted at the same time from two independent DbContext instances (i.e.
/// two independent connections/transactions, exactly like two browser tabs). Only one may
/// succeed — PaymentService's SELECT ... FOR UPDATE row lock serializes the second request
/// behind the first, so it re-validates against the post-commit OutstandingAmount and is
/// rejected rather than silently overpaying.
/// </summary>
public class ConcurrentPaymentTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ConcurrentPaymentTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [MySqlAvailableFact]
    public async Task TwoConcurrentPayments_ThatWouldJointlyOverpay_OnlyOneSucceeds()
    {
        await using var seedDb = _fixture.CreateContext();

        var branch = new Branch { BranchCode = $"CNC-{Guid.NewGuid():N}"[..12], Name = "Concurrency Branch", PhoneNumber = "0", Address = "n/a", City = "n/a", CreatedAtUtc = DateTime.UtcNow };
        seedDb.Branches.Add(branch);

        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = $"u-{Guid.NewGuid():N}@t.local", Email = $"u-{Guid.NewGuid():N}@t.local", FullName = "Cashier", EmployeeCode = $"E-{Guid.NewGuid():N}"[..15], CreatedAtUtc = DateTime.UtcNow };
        seedDb.Users.Add(user);
        await seedDb.SaveChangesAsync();

        var customer = new Customer { BranchId = branch.BranchId, CustomerCode = $"C-{Guid.NewGuid():N}"[..15], FullName = "Concurrency Customer", PhoneNumber = "0", Address = "n/a", City = "n/a", RegistrationDate = DateTime.UtcNow, Status = CustomerStatus.Active, CreatedAtUtc = DateTime.UtcNow };
        seedDb.Customers.Add(customer);
        await seedDb.SaveChangesAsync();

        seedDb.CustomerAccounts.Add(new CustomerAccount { CustomerId = customer.CustomerId, CreditLimit = 0, TotalBilled = 1000m, TotalPaid = 0, CurrentBalance = 1000m, AccountStatus = AccountStatus.Active, CreatedAtUtc = DateTime.UtcNow });

        var invoice = new Invoice
        {
            CustomerId = customer.CustomerId,
            BranchId = branch.BranchId,
            InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..15],
            InvoiceDate = DateTime.UtcNow,
            Subtotal = 1000m,
            TotalAmount = 1000m,
            OutstandingAmount = 1000m, // exactly enough for ONE of the two 700 payments below
            PaymentStatus = PaymentStatus.Unpaid,
            InvoiceStatus = InvoiceStatus.Active,
            CreatedByUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow
        };
        seedDb.Invoices.Add(invoice);
        await seedDb.SaveChangesAsync();

        var currentUser = FakeCurrentUserContext.ForBranch(branch.BranchId);
        currentUser.UserId = user.Id;

        // Two independent DbContexts = two independent connections/transactions, just like
        // two different HTTP requests would each get their own scoped ApplicationDbContext.
        await using var dbA = _fixture.CreateContext();
        await using var dbB = _fixture.CreateContext();
        var serviceA = new PaymentService(dbA, currentUser, new AuditLogService(dbA));
        var serviceB = new PaymentService(dbB, currentUser, new AuditLogService(dbB));

        var paymentA = Task.Run(() => serviceA.RecordPaymentAsync(new Payment
        {
            InvoiceId = invoice.InvoiceId,
            PaymentNumber = $"PAY-A-{Guid.NewGuid():N}"[..15],
            Amount = 700m,
            PaymentMethod = PaymentMethod.Cash
        }));

        var paymentB = Task.Run(() => serviceB.RecordPaymentAsync(new Payment
        {
            InvoiceId = invoice.InvoiceId,
            PaymentNumber = $"PAY-B-{Guid.NewGuid():N}"[..15],
            Amount = 700m,
            PaymentMethod = PaymentMethod.Cash
        }));

        var results = await Task.WhenAll(paymentA.ContinueWith(t => t), paymentB.ContinueWith(t => t));

        var succeeded = results.Count(t => !t.IsFaulted);
        var failedWithBusinessRule = results.Count(t =>
            t.IsFaulted && t.Exception!.InnerExceptions.Any(e => e is BusinessRuleException));

        Assert.Equal(1, succeeded);
        Assert.Equal(1, failedWithBusinessRule);

        await using var verifyDb = _fixture.CreateContext();
        var finalInvoice = await verifyDb.Invoices.FindAsync(invoice.InvoiceId);
        Assert.Equal(700m, finalInvoice!.PaidAmount); // exactly one payment applied, never both
        Assert.True(finalInvoice.OutstandingAmount >= 0);
    }
}
