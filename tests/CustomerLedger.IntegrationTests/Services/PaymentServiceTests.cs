using CustomerLedger.Application.Exceptions;
using CustomerLedger.DatabaseTests;
using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using CustomerLedger.Infrastructure.Services;
using CustomerLedger.IntegrationTests.Fakes;

namespace CustomerLedger.IntegrationTests.Services;

public class PaymentServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PaymentServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(ApplicationDbContext Db, Invoice Invoice, FakeCurrentUserContext User)> SeedActiveInvoiceAsync(ApplicationDbContext db, decimal totalAmount = 1000m)
    {
        var branch = new Branch { BranchCode = $"PAY-{Guid.NewGuid():N}"[..12], Name = "Payment Test Branch", PhoneNumber = "0", Address = "n/a", City = "n/a", CreatedAtUtc = DateTime.UtcNow };
        db.Branches.Add(branch);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"user-{Guid.NewGuid():N}@test.local",
            Email = $"user-{Guid.NewGuid():N}@test.local",
            FullName = "Test Cashier",
            EmployeeCode = $"EMP-{Guid.NewGuid():N}"[..15],
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        user.BranchId = branch.BranchId;

        var customer = new Customer
        {
            BranchId = branch.BranchId,
            CustomerCode = $"CUST-{Guid.NewGuid():N}"[..15],
            FullName = "Payment Test Customer",
            PhoneNumber = "0",
            Address = "n/a",
            City = "n/a",
            RegistrationDate = DateTime.UtcNow,
            Status = CustomerStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var invoice = new Invoice
        {
            CustomerId = customer.CustomerId,
            BranchId = branch.BranchId,
            InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..15],
            InvoiceDate = DateTime.UtcNow,
            Subtotal = totalAmount,
            TotalAmount = totalAmount,
            OutstandingAmount = totalAmount,
            PaymentStatus = PaymentStatus.Unpaid,
            InvoiceStatus = InvoiceStatus.Active,
            CreatedByUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var currentUser = FakeCurrentUserContext.ForBranch(branch.BranchId);
        currentUser.UserId = user.Id;

        return (db, invoice, currentUser);
    }

    [MySqlAvailableFact]
    public async Task RecordPaymentAsync_FullPayment_MarksInvoicePaid()
    {
        await using var db = _fixture.CreateContext();
        var (_, invoice, currentUser) = await SeedActiveInvoiceAsync(db, 1000m);
        var sut = new PaymentService(db, currentUser, new AuditLogService(db));

        await sut.RecordPaymentAsync(new Payment
        {
            InvoiceId = invoice.InvoiceId,
            PaymentNumber = $"PAY-{Guid.NewGuid():N}"[..15],
            Amount = 1000m,
            PaymentMethod = PaymentMethod.Cash
        });

        var updated = await db.Invoices.FindAsync(invoice.InvoiceId);
        Assert.Equal(PaymentStatus.Paid, updated!.PaymentStatus);
        Assert.Equal(0m, updated.OutstandingAmount);
    }

    [MySqlAvailableFact]
    public async Task RecordPaymentAsync_PartialPayment_MarksInvoicePartiallyPaid()
    {
        await using var db = _fixture.CreateContext();
        var (_, invoice, currentUser) = await SeedActiveInvoiceAsync(db, 1000m);
        var sut = new PaymentService(db, currentUser, new AuditLogService(db));

        await sut.RecordPaymentAsync(new Payment
        {
            InvoiceId = invoice.InvoiceId,
            PaymentNumber = $"PAY-{Guid.NewGuid():N}"[..15],
            Amount = 400m,
            PaymentMethod = PaymentMethod.Cash
        });

        var updated = await db.Invoices.FindAsync(invoice.InvoiceId);
        Assert.Equal(PaymentStatus.PartiallyPaid, updated!.PaymentStatus);
        Assert.Equal(600m, updated.OutstandingAmount);
    }

    [MySqlAvailableFact]
    public async Task RecordPaymentAsync_ZeroAmount_ThrowsBusinessRuleException()
    {
        await using var db = _fixture.CreateContext();
        var (_, invoice, currentUser) = await SeedActiveInvoiceAsync(db);
        var sut = new PaymentService(db, currentUser, new AuditLogService(db));

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.RecordPaymentAsync(new Payment
        {
            InvoiceId = invoice.InvoiceId,
            PaymentNumber = $"PAY-{Guid.NewGuid():N}"[..15],
            Amount = 0m,
            PaymentMethod = PaymentMethod.Cash
        }));
    }

    [MySqlAvailableFact]
    public async Task RecordPaymentAsync_Overpayment_ThrowsBusinessRuleException()
    {
        await using var db = _fixture.CreateContext();
        var (_, invoice, currentUser) = await SeedActiveInvoiceAsync(db, 1000m);
        var sut = new PaymentService(db, currentUser, new AuditLogService(db));

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.RecordPaymentAsync(new Payment
        {
            InvoiceId = invoice.InvoiceId,
            PaymentNumber = $"PAY-{Guid.NewGuid():N}"[..15],
            Amount = 1500m,
            PaymentMethod = PaymentMethod.Cash
        }));
    }

    [MySqlAvailableFact]
    public async Task RecordPaymentAsync_AgainstCancelledInvoice_ThrowsBusinessRuleException()
    {
        await using var db = _fixture.CreateContext();
        var (_, invoice, currentUser) = await SeedActiveInvoiceAsync(db);
        invoice.InvoiceStatus = InvoiceStatus.Cancelled;
        await db.SaveChangesAsync();

        var sut = new PaymentService(db, currentUser, new AuditLogService(db));

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.RecordPaymentAsync(new Payment
        {
            InvoiceId = invoice.InvoiceId,
            PaymentNumber = $"PAY-{Guid.NewGuid():N}"[..15],
            Amount = 100m,
            PaymentMethod = PaymentMethod.Cash
        }));
    }

    [MySqlAvailableFact]
    public async Task RecordPaymentAsync_FromDifferentBranch_ThrowsBranchAccessDeniedException()
    {
        await using var db = _fixture.CreateContext();
        var (_, invoice, _) = await SeedActiveInvoiceAsync(db);
        var otherBranchUser = FakeCurrentUserContext.ForBranch(invoice.BranchId + 999_999);
        var sut = new PaymentService(db, otherBranchUser, new AuditLogService(db));

        await Assert.ThrowsAsync<BranchAccessDeniedException>(() => sut.RecordPaymentAsync(new Payment
        {
            InvoiceId = invoice.InvoiceId,
            PaymentNumber = $"PAY-{Guid.NewGuid():N}"[..15],
            Amount = 100m,
            PaymentMethod = PaymentMethod.Cash
        }));
    }
}
