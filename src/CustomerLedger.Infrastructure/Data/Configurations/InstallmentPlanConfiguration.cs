using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class InstallmentPlanConfiguration : IEntityTypeConfiguration<InstallmentPlan>
{
    public void Configure(EntityTypeBuilder<InstallmentPlan> builder)
    {
        builder.ToTable("InstallmentPlans");

        builder.HasKey(p => p.InstallmentPlanId);

        builder.Property(p => p.TotalInstallmentAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.DownPayment).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Frequency).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CreatedAtUtc).IsRequired();

        // An invoice must not have multiple simultaneously active installment plans —
        // enforced here via the one-to-one FK on InvoiceId (see InvoiceConfiguration).
        builder.HasIndex(p => p.InvoiceId).IsUnique();

        builder.HasOne(p => p.ApprovedByUser)
            .WithMany()
            .HasForeignKey(p => p.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Schedules)
            .WithOne(s => s.InstallmentPlan)
            .HasForeignKey(s => s.InstallmentPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
