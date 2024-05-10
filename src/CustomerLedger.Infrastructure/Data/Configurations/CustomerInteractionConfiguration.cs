using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class CustomerInteractionConfiguration : IEntityTypeConfiguration<CustomerInteraction>
{
    public void Configure(EntityTypeBuilder<CustomerInteraction> builder)
    {
        builder.ToTable("CustomerInteractions");

        builder.HasKey(ci => ci.CustomerInteractionId);

        builder.Property(ci => ci.InteractionType).HasConversion<string>().HasMaxLength(30);
        builder.Property(ci => ci.Subject).IsRequired().HasMaxLength(200);
        builder.Property(ci => ci.Description).IsRequired().HasMaxLength(2000);
        builder.Property(ci => ci.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(ci => ci.RecordedByUserId).IsRequired();
        builder.Property(ci => ci.CreatedAtUtc).IsRequired();

        // Supports CustomerInteractions(CustomerId, InteractionDate).
        builder.HasIndex(ci => new { ci.CustomerId, ci.InteractionDate });

        builder.HasOne(ci => ci.RecordedByUser)
            .WithMany()
            .HasForeignKey(ci => ci.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
