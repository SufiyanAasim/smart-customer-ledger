using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.PaymentNumber).IsRequired().HasMaxLength(30);
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.TransactionReference).HasMaxLength(100);
        builder.Property(p => p.PaymentStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ReceivedByUserId).IsRequired();
        builder.Property(p => p.ReversalReason).HasMaxLength(500);
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.Property(p => p.CreatedAtUtc).IsRequired();

        // Supports Payments(PaymentNumber), (InvoiceId, PaymentStatus),
        // (CustomerId, PaymentDate) and (BranchId, PaymentDate) reporting queries.
        builder.HasIndex(p => p.PaymentNumber).IsUnique();
        builder.HasIndex(p => new { p.InvoiceId, p.PaymentStatus });
        builder.HasIndex(p => new { p.CustomerId, p.PaymentDate });
        builder.HasIndex(p => new { p.BranchId, p.PaymentDate });

        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Branch)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ReceivedByUser)
            .WithMany()
            .HasForeignKey(p => p.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing reversal link — restrict delete so a reversed payment can
        // never vanish out from under the reversal record that points at it.
        builder.HasOne(p => p.ReversedPayment)
            .WithMany()
            .HasForeignKey(p => p.ReversedPaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
