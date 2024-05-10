using CustomerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerLedger.Infrastructure.Data.Configurations;

public class CustomerAccountConfiguration : IEntityTypeConfiguration<CustomerAccount>
{
    public void Configure(EntityTypeBuilder<CustomerAccount> builder)
    {
        builder.ToTable("CustomerAccounts");

        builder.HasKey(a => a.CustomerAccountId);

        builder.Property(a => a.CreditLimit).HasColumnType("decimal(18,2)");
        builder.Property(a => a.CurrentBalance).HasColumnType("decimal(18,2)");
        builder.Property(a => a.TotalBilled).HasColumnType("decimal(18,2)");
        builder.Property(a => a.TotalPaid).HasColumnType("decimal(18,2)");
        builder.Property(a => a.AccountStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.ConcurrencyVersion).IsConcurrencyToken();

        // One customer must have exactly one financial account.
        builder.HasIndex(a => a.CustomerId).IsUnique();
    }
}
