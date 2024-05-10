using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.BranchId);

        builder.Property(b => b.BranchCode).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
        builder.Property(b => b.Email).HasMaxLength(256);
        builder.Property(b => b.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(300);
        builder.Property(b => b.City).IsRequired().HasMaxLength(100);
        builder.Property(b => b.IsActive).HasDefaultValue(true);
        builder.Property(b => b.CreatedAtUtc).IsRequired();

        // Supports lookup by code during login/branch assignment (Branches(BranchCode)).
        builder.HasIndex(b => b.BranchCode).IsUnique();

        builder.HasMany(b => b.Customers)
            .WithOne(c => c.Branch)
            .HasForeignKey(c => c.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Invoices)
            .WithOne(i => i.Branch)
            .HasForeignKey(i => i.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Payments)
            .WithOne(p => p.Branch)
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Interactions)
            .WithOne(ci => ci.Branch)
            .HasForeignKey(ci => ci.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
