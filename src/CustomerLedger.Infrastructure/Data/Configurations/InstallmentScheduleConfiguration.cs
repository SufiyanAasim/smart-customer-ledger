using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class InstallmentScheduleConfiguration : IEntityTypeConfiguration<InstallmentSchedule>
{
    public void Configure(EntityTypeBuilder<InstallmentSchedule> builder)
    {
        builder.ToTable("InstallmentSchedules");

        builder.HasKey(s => s.InstallmentScheduleId);

        builder.Property(s => s.AmountDue).HasColumnType("decimal(18,2)");
        builder.Property(s => s.AmountPaid).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.CreatedAtUtc).IsRequired();

        // InstallmentNumber must be unique within a plan.
        builder.HasIndex(s => new { s.InstallmentPlanId, s.InstallmentNumber }).IsUnique();

        // Supports InstallmentSchedules(Status, DueDate) used by vw_OverdueInstallments.
        builder.HasIndex(s => new { s.Status, s.DueDate });
    }
}
