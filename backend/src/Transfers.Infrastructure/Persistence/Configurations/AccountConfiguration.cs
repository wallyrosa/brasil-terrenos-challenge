using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfers.Domain.Accounts;

namespace Transfers.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", table =>
            table.HasCheckConstraint("ck_accounts_overdraft_limit", "overdraft_limit >= 0"));

        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).ValueGeneratedNever();

        builder.Property(account => account.HolderName).HasMaxLength(200).IsRequired();
        builder.Property(account => account.Document).HasMaxLength(20).IsRequired();
        builder.HasIndex(account => account.Document).IsUnique();

        builder.Property(account => account.Balance).HasPrecision(18, 2);
        builder.Property(account => account.OverdraftLimit).HasPrecision(18, 2);
        builder.Property(account => account.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(account => account.CreatedAt);

        builder.ComplexProperty(account => account.TransferLimits, limits =>
        {
            limits.ComplexProperty(limit => limit.Daytime, daytime =>
            {
                daytime.Property(hourly => hourly.MaxAmountPerHour)
                    .HasColumnName("daytime_max_amount_per_hour")
                    .HasPrecision(18, 2);
                daytime.Property(hourly => hourly.MaxAttemptsPerHour)
                    .HasColumnName("daytime_max_attempts_per_hour");
            });

            limits.ComplexProperty(limit => limit.Nighttime, nighttime =>
            {
                nighttime.Property(hourly => hourly.MaxAmountPerHour)
                    .HasColumnName("nighttime_max_amount_per_hour")
                    .HasPrecision(18, 2);
                nighttime.Property(hourly => hourly.MaxAttemptsPerHour)
                    .HasColumnName("nighttime_max_attempts_per_hour");
            });
        });

        builder.Ignore(account => account.DomainEvents);
    }
}
