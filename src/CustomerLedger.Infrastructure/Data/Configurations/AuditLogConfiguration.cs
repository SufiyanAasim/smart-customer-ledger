using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.AuditLogId);

        builder.Property(a => a.TableName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.RecordId).IsRequired().HasMaxLength(50);
        builder.Property(a => a.ActionType).IsRequired().HasMaxLength(30);
        builder.Property(a => a.OldValuesJson).HasColumnType("longtext");
        builder.Property(a => a.NewValuesJson).HasColumnType("longtext");
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.Property(a => a.ReviewStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.AdminNote).HasMaxLength(1000);
        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.IsArchived).HasDefaultValue(false);

        // Supports AuditLogs(TableName, RecordId) and (BranchId, CreatedAtUtc).
        builder.HasIndex(a => new { a.TableName, a.RecordId });
        builder.HasIndex(a => new { a.BranchId, a.CreatedAtUtc });

        // No navigation-based FK constraints to Identity users on purpose: audit rows must
        // remain even if the referenced user or branch is later removed from the system.
        builder.Property(a => a.UserId).HasMaxLength(450);
    }
}
