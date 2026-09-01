using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.ComplexProperty(a => a.Balance, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Balance")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.HasIndex(a => a.UserId);
    }
}
