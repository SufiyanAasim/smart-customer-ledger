using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class BackupHistoryConfiguration : IEntityTypeConfiguration<BackupHistory>
{
    public void Configure(EntityTypeBuilder<BackupHistory> builder)
    {
        builder.ToTable("BackupHistories");

        builder.HasKey(b => b.BackupHistoryId);

        builder.Property(b => b.BackupType).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.FileName).IsRequired().HasMaxLength(260);
        builder.Property(b => b.FilePath).IsRequired().HasMaxLength(1000);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.ErrorMessage).HasMaxLength(2000);
        builder.Property(b => b.CreatedByUserId).IsRequired();
        builder.Property(b => b.CreatedAtUtc).IsRequired();

        builder.HasOne(b => b.CreatedByUser)
            .WithMany()
            .HasForeignKey(b => b.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
