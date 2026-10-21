using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transfers.Domain.Accounts;
using Transfers.Domain.Transfers;

namespace Transfers.Infrastructure.Persistence.Configurations;

internal sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("transfers", table =>
            table.HasCheckConstraint("ck_transfers_amount_positive", "amount > 0"));

        builder.HasKey(transfer => transfer.Id);
        builder.Property(transfer => transfer.Id).ValueGeneratedNever();

        builder.Property(transfer => transfer.Amount).HasPrecision(18, 2);
        builder.Property(transfer => transfer.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(transfer => transfer.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(transfer => transfer.FailureReason).HasConversion<string>().HasMaxLength(50);
        builder.Property(transfer => transfer.IdempotencyKey).HasMaxLength(100);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(transfer => transfer.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(transfer => transfer.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transfer => transfer.IdempotencyKey).IsUnique();
        builder.HasIndex(transfer => new { transfer.SourceAccountId, transfer.ProcessedAt });
        builder.HasIndex(transfer => new { transfer.Status, transfer.ScheduledFor });

        builder.Ignore(transfer => transfer.DomainEvents);
    }
}
