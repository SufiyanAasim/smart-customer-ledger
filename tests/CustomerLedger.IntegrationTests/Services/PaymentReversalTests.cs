using CustomerLedger.Application.Exceptions;
using CustomerLedger.DatabaseTests;
using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Services;
using CustomerLedger.IntegrationTests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.IntegrationTests.Services;

public class PaymentReversalTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PaymentReversalTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(Invoice Invoice, Payment Payment, FakeCurrentUserContext User, Infrastructure.Data.Contexts.ApplicationDbContext Db)> SeedInvoiceWithPaymentAsync(Infrastructure.Data.Contexts.ApplicationDbContext db, decimal invoiceTotal = 1000m, decimal paymentAmount = 400m)
    {
        var branch = new Branch { BranchCode = $"REV-{Guid.NewGuid():N}"[..12], Name = "Reversal Branch", PhoneNumber = "0", Address = "n/a", City = "n/a", CreatedAtUtc = DateTime.UtcNow };
        db.Branches.Add(branch);

        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = $"u-{Guid.NewGuid():N}@t.local", Email = $"u-{Guid.NewGuid():N}@t.local", FullName = "Cashier", EmployeeCode = $"E-{Guid.NewGuid():N}"[..15], CreatedAtUtc = DateTime.UtcNow };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var customer = new Customer { BranchId = branch.BranchId, CustomerCode = $"C-{Guid.NewGuid():N}"[..15], FullName = "Reversal Customer", PhoneNumber = "0", Address = "n/a", City = "n/a", RegistrationDate = DateTime.UtcNow, Status = CustomerStatus.Active, CreatedAtUtc = DateTime.UtcNow };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        db.CustomerAccounts.Add(new CustomerAccount { CustomerId = customer.CustomerId, CreditLimit = 0, TotalBilled = invoiceTotal, TotalPaid = 0, CurrentBalance = invoiceTotal, AccountStatus = AccountStatus.Active, CreatedAtUtc = DateTime.UtcNow });

        var invoice = new Invoice
        {
            CustomerId = customer.CustomerId,
            BranchId = branch.BranchId,
            InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..15],
            InvoiceDate = DateTime.UtcNow,
            Subtotal = invoiceTotal,
            TotalAmount = invoiceTotal,
            OutstandingAmount = invoiceTotal,
            PaymentStatus = PaymentStatus.Unpaid,
            InvoiceStatus = InvoiceStatus.Active,
            CreatedByUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var currentUser = FakeCurrentUserContext.ForBranch(branch.BranchId);
        currentUser.UserId = user.Id;

        var paymentService = new PaymentService(db, currentUser, new AuditLogService(db));
        var payment = await paymentService.RecordPaymentAsync(new Payment
        {
            InvoiceId = invoice.InvoiceId,
            PaymentNumber = $"PAY-{Guid.NewGuid():N}"[..15],
            Amount = paymentAmount,
            PaymentMethod = PaymentMethod.Cash
        });

        return (invoice, payment, currentUser, db);
    }

    [MySqlAvailableFact]
    public async Task ReverseAsync_RestoresInvoiceAndAccountBalances()
    {
        await using var db = _fixture.CreateContext();
        var (invoice, payment, currentUser, _) = await SeedInvoiceWithPaymentAsync(db, 1000m, 400m);
        var sut = new PaymentService(db, currentUser, new AuditLogService(db));

        await sut.ReverseAsync(payment.PaymentId, "Customer disputed the charge");

        var updatedInvoice = await db.Invoices.FindAsync(invoice.InvoiceId);
        Assert.Equal(1000m, updatedInvoice!.OutstandingAmount);
        Assert.Equal(PaymentStatus.Unpaid, updatedInvoice.PaymentStatus);

        var account = await db.CustomerAccounts.FirstAsync(a => a.CustomerId == invoice.CustomerId);
        Assert.Equal(0m, account.TotalPaid);
        Assert.Equal(1000m, account.CurrentBalance);
    }

    [MySqlAvailableFact]
    public async Task ReverseAsync_CalledTwice_ThrowsBusinessRuleException()
    {
        await using var db = _fixture.CreateContext();
        var (_, payment, currentUser, _) = await SeedInvoiceWithPaymentAsync(db);
        var sut = new PaymentService(db, currentUser, new AuditLogService(db));

        await sut.ReverseAsync(payment.PaymentId, "First reversal");

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.ReverseAsync(payment.PaymentId, "Second attempt"));
    }

    [MySqlAvailableFact]
    public async Task ReverseAsync_WithoutReason_ThrowsBusinessRuleException()
    {
        await using var db = _fixture.CreateContext();
        var (_, payment, currentUser, _) = await SeedInvoiceWithPaymentAsync(db);
        var sut = new PaymentService(db, currentUser, new AuditLogService(db));

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.ReverseAsync(payment.PaymentId, ""));
    }
}
