using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");

        builder.HasKey(ii => ii.InvoiceItemId);

        builder.Property(ii => ii.Description).IsRequired().HasMaxLength(300);
        builder.Property(ii => ii.Quantity).HasColumnType("decimal(18,2)");
        builder.Property(ii => ii.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(ii => ii.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(ii => ii.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(ii => ii.LineTotal).HasColumnType("decimal(18,2)");
        builder.Property(ii => ii.CreatedAtUtc).IsRequired();

        builder.HasIndex(ii => ii.InvoiceId);
    }
}
