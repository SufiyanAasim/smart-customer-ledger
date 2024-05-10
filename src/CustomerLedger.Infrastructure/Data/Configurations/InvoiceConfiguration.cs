using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.InvoiceId);

        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(30);
        builder.Property(i => i.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.OutstandingAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.PaymentStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.InvoiceStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.CreatedByUserId).IsRequired();
        builder.Property(i => i.IsDeleted).HasDefaultValue(false);
        builder.Property(i => i.CreatedAtUtc).IsRequired();
        builder.Property(i => i.ConcurrencyVersion).IsConcurrencyToken();

        // Supports Invoices(InvoiceNumber), (CustomerId, PaymentStatus),
        // (BranchId, InvoiceDate) and (BranchId, InvoiceStatus, InvoiceDate) reporting queries.
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => new { i.CustomerId, i.PaymentStatus });
        builder.HasIndex(i => new { i.BranchId, i.InvoiceDate });
        builder.HasIndex(i => new { i.BranchId, i.InvoiceStatus, i.InvoiceDate });

        builder.HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Branch)
            .WithMany(b => b.Invoices)
            .HasForeignKey(i => i.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.CreatedByUser)
            .WithMany()
            .HasForeignKey(i => i.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invoice items are owned by the invoice header; cascading here only removes
        // draft line items together with their (never-finalized) parent invoice.
        builder.HasMany(i => i.InvoiceItems)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.InstallmentPlan)
            .WithOne(p => p.Invoice)
            .HasForeignKey<InstallmentPlan>(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
