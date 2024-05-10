using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.CustomerId);

        builder.Property(c => c.CustomerCode).IsRequired().HasMaxLength(20);
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.CNIC).HasMaxLength(20);
        builder.Property(c => c.Address).IsRequired().HasMaxLength(300);
        builder.Property(c => c.City).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.Property(c => c.CreatedAtUtc).IsRequired();

        // Supports Customers(CustomerCode), (PhoneNumber), (CNIC) and the composite
        // branch/status/soft-delete filter used by every customer list screen.
        builder.HasIndex(c => c.CustomerCode).IsUnique();
        builder.HasIndex(c => c.PhoneNumber);
        builder.HasIndex(c => c.CNIC);
        builder.HasIndex(c => new { c.BranchId, c.Status, c.IsDeleted });

        builder.HasOne(c => c.Branch)
            .WithMany(b => b.Customers)
            .HasForeignKey(c => c.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CustomerAccount)
            .WithOne(a => a.Customer)
            .HasForeignKey<CustomerAccount>(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Invoices)
            .WithOne(i => i.Customer)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Interactions)
            .WithOne(ci => ci.Customer)
            .HasForeignKey(ci => ci.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
