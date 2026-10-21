using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Transfers.Infrastructure.Persistence;

#nullable disable

namespace Transfers.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(TransfersDbContext))]
    partial class TransfersDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Transfers.Domain.Accounts.Account", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("Balance")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)")
                        .HasColumnName("balance");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Document")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("document");

                    b.Property<string>("HolderName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("holder_name");

                    b.Property<decimal>("OverdraftLimit")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)")
                        .HasColumnName("overdraft_limit");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("status");

                    b.ComplexProperty(typeof(Dictionary<string, object>), "TransferLimits", "Transfers.Domain.Accounts.Account.TransferLimits#TransferLimits", b1 =>
                        {
                            b1.IsRequired();

                            b1.ComplexProperty(typeof(Dictionary<string, object>), "Daytime", "Transfers.Domain.Accounts.Account.TransferLimits#TransferLimits.Daytime#HourlyLimit", b2 =>
                                {
                                    b2.IsRequired();

                                    b2.Property<decimal>("MaxAmountPerHour")
                                        .HasPrecision(18, 2)
                                        .HasColumnType("numeric(18,2)")
                                        .HasColumnName("daytime_max_amount_per_hour");

                                    b2.Property<int>("MaxAttemptsPerHour")
                                        .HasColumnType("integer")
                                        .HasColumnName("daytime_max_attempts_per_hour");
                                });

                            b1.ComplexProperty(typeof(Dictionary<string, object>), "Nighttime", "Transfers.Domain.Accounts.Account.TransferLimits#TransferLimits.Nighttime#HourlyLimit", b2 =>
                                {
                                    b2.IsRequired();

                                    b2.Property<decimal>("MaxAmountPerHour")
                                        .HasPrecision(18, 2)
                                        .HasColumnType("numeric(18,2)")
                                        .HasColumnName("nighttime_max_amount_per_hour");

                                    b2.Property<int>("MaxAttemptsPerHour")
                                        .HasColumnType("integer")
                                        .HasColumnName("nighttime_max_attempts_per_hour");
                                });
                        });

                    b.HasKey("Id")
                        .HasName("pk_accounts");

                    b.HasIndex("Document")
                        .IsUnique()
                        .HasDatabaseName("ix_accounts_document");

                    b.ToTable("accounts", null, t =>
                        {
                            t.HasCheckConstraint("ck_accounts_overdraft_limit", "overdraft_limit >= 0");
                        });
                });

            modelBuilder.Entity("Transfers.Domain.Transfers.Transfer", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("Amount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)")
                        .HasColumnName("amount");

                    b.Property<DateTimeOffset?>("CancelledAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("cancelled_at");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<Guid>("DestinationAccountId")
                        .HasColumnType("uuid")
                        .HasColumnName("destination_account_id");

                    b.Property<string>("FailureReason")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("failure_reason");

                    b.Property<string>("IdempotencyKey")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("idempotency_key");

                    b.Property<DateTimeOffset?>("ProcessedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_at");

                    b.Property<DateTimeOffset?>("ScheduledFor")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("scheduled_for");

                    b.Property<Guid>("SourceAccountId")
                        .HasColumnType("uuid")
                        .HasColumnName("source_account_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("status");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_transfers");

                    b.HasIndex("DestinationAccountId")
                        .HasDatabaseName("ix_transfers_destination_account_id");

                    b.HasIndex("IdempotencyKey")
                        .IsUnique()
                        .HasDatabaseName("ix_transfers_idempotency_key");

                    b.HasIndex("SourceAccountId", "ProcessedAt")
                        .HasDatabaseName("ix_transfers_source_account_id_processed_at");

                    b.HasIndex("Status", "ScheduledFor")
                        .HasDatabaseName("ix_transfers_status_scheduled_for");

                    b.ToTable("transfers", null, t =>
                        {
                            t.HasCheckConstraint("ck_transfers_amount_positive", "amount > 0");
                        });
                });

            modelBuilder.Entity("Transfers.Domain.Transfers.Transfer", b =>
                {
                    b.HasOne("Transfers.Domain.Accounts.Account", null)
                        .WithMany()
                        .HasForeignKey("DestinationAccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_transfers_accounts_destination_account_id");

                    b.HasOne("Transfers.Domain.Accounts.Account", null)
                        .WithMany()
                        .HasForeignKey("SourceAccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_transfers_accounts_source_account_id");
                });
#pragma warning restore 612, 618
        }
    }
}
