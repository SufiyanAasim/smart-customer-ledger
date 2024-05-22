using CustomerLedger.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Data.Contexts;

/// <summary>
/// The single EF Core context for CustomerLedger. Identity tables (AspNetUsers, AspNetRoles,
/// etc.) live alongside business tables in the same MySQL database — there is one source of
/// truth, configured entirely through Fluent API in Data/Configurations.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Non-generic constructor so ReplicaDbContext (same model, different connection
    /// string) can subclass this type and pass its own DbContextOptions&lt;ReplicaDbContext&gt;
    /// through — the standard EF Core pattern for "two DbContexts sharing one model".
    /// </summary>
    protected ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAccount> CustomerAccounts => Set<CustomerAccount>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InstallmentPlan> InstallmentPlans => Set<InstallmentPlan>();
    public DbSet<InstallmentSchedule> InstallmentSchedules => Set<InstallmentSchedule>();
    public DbSet<CustomerInteraction> CustomerInteractions => Set<CustomerInteraction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<BackupHistory> BackupHistories => Set<BackupHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
