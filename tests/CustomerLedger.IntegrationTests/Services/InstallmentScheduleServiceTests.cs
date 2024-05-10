using CustomerLedger.DatabaseTests;
using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Services;
using CustomerLedger.IntegrationTests.Fakes;

namespace CustomerLedger.IntegrationTests.Services;

public class InstallmentScheduleServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public InstallmentScheduleServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [MySqlAvailableFact]
    public async Task PayInstallmentAsync_FullyPaidRow_MarksScheduleAndPlanComplete()
    {
        await using var db = _fixture.CreateContext();

        var branch = new Branch { BranchCode = $"INS-{Guid.NewGuid():N}"[..12], Name = "Installment Branch", PhoneNumber = "0", Address = "n/a", City = "n/a", CreatedAtUtc = DateTime.UtcNow };
        db.Branches.Add(branch);

        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = $"u-{Guid.NewGuid():N}@t.local", Email = $"u-{Guid.NewGuid():N}@t.local", FullName = "Cashier", EmployeeCode = $"E-{Guid.NewGuid():N}"[..15], CreatedAtUtc = DateTime.UtcNow };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var customer = new Customer { BranchId = branch.BranchId, CustomerCode = $"C-{Guid.NewGuid():N}"[..15], FullName = "Installment Customer", PhoneNumber = "0", Address = "n/a", City = "n/a", RegistrationDate = DateTime.UtcNow, Status = CustomerStatus.Active, CreatedAtUtc = DateTime.UtcNow };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        db.CustomerAccounts.Add(new CustomerAccount { CustomerId = customer.CustomerId, CreditLimit = 0, TotalBilled = 1000m, TotalPaid = 0, CurrentBalance = 1000m, AccountStatus = AccountStatus.Active, CreatedAtUtc = DateTime.UtcNow });

        var invoice = new Invoice
        {
            CustomerId = customer.CustomerId,
            BranchId = branch.BranchId,
            InvoiceNumber = $"INV-{Guid.NewGuid():N}"[..15],
            InvoiceDate = DateTime.UtcNow,
            Subtotal = 1000m,
            TotalAmount = 1000m,
            OutstandingAmount = 1000m,
            PaymentStatus = PaymentStatus.Unpaid,
            InvoiceStatus = InvoiceStatus.Active,
            CreatedByUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var plan = new InstallmentPlan
        {
            InvoiceId = invoice.InvoiceId,
            NumberOfInstallments = 2,
            TotalInstallmentAmount = 1000m,
            DownPayment = 0,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(2),
            Frequency = InstallmentFrequency.Monthly,
            Status = InstallmentPlanStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.InstallmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var schedule1 = new InstallmentSchedule { InstallmentPlanId = plan.InstallmentPlanId, InstallmentNumber = 1, DueDate = DateTime.UtcNow.AddMonths(1), AmountDue = 500m, AmountPaid = 0, Status = InstallmentStatus.Pending, CreatedAtUtc = DateTime.UtcNow };
        var schedule2 = new InstallmentSchedule { InstallmentPlanId = plan.InstallmentPlanId, InstallmentNumber = 2, DueDate = DateTime.UtcNow.AddMonths(2), AmountDue = 500m, AmountPaid = 0, Status = InstallmentStatus.Pending, CreatedAtUtc = DateTime.UtcNow };
        db.InstallmentSchedules.AddRange(schedule1, schedule2);
        await db.SaveChangesAsync();

        var currentUser = FakeCurrentUserContext.ForBranch(branch.BranchId);
        currentUser.UserId = user.Id;
        var paymentService = new PaymentService(db, currentUser, new AuditLogService(db));
        var sut = new InstallmentScheduleService(db, currentUser, paymentService);

        await sut.PayInstallmentAsync(schedule1.InstallmentScheduleId, 500m, PaymentMethod.Cash, null);

        var updatedSchedule = await db.InstallmentSchedules.FindAsync(schedule1.InstallmentScheduleId);
        Assert.Equal(InstallmentStatus.Paid, updatedSchedule!.Status);
        Assert.NotNull(updatedSchedule.PaidDate);

        var updatedInvoice = await db.Invoices.FindAsync(invoice.InvoiceId);
        Assert.Equal(500m, updatedInvoice!.OutstandingAmount);

        var updatedPlan = await db.InstallmentPlans.FindAsync(plan.InstallmentPlanId);
        Assert.Equal(InstallmentPlanStatus.Active, updatedPlan!.Status); // one row still pending

        await sut.PayInstallmentAsync(schedule2.InstallmentScheduleId, 500m, PaymentMethod.Cash, null);

        var completedPlan = await db.InstallmentPlans.FindAsync(plan.InstallmentPlanId);
        Assert.Equal(InstallmentPlanStatus.Completed, completedPlan!.Status);
    }
}
