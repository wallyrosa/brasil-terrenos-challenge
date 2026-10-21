using FluentAssertions;
using Transfers.Domain.Accounts;
using Transfers.Domain.Limits;
using Transfers.Domain.Transfers;
using Transfers.Domain.Transfers.Events;

namespace Transfers.UnitTests.Domain;

public sealed class TransferExecutionServiceTests
{
    private readonly TransferExecutionService _service = new();

    [Fact]
    public void Execute_WithSufficientBalance_CompletesAndMovesMoney()
    {
        var (source, destination) = Parties(sourceBalance: 1000);
        var transfer = Immediate(source, destination, 300);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, HourlyUsage.None, TestData.Now);

        transfer.Status.Should().Be(TransferStatus.Completed);
        transfer.ProcessedAt.Should().Be(TestData.Now);
        source.Balance.Should().Be(700);
        destination.Balance.Should().Be(300);
        transfer.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TransferCompleted>();
    }

    [Fact]
    public void Execute_UsingOverdraft_CompletesWithNegativeBalance()
    {
        var (source, destination) = Parties(sourceBalance: 500, sourceOverdraft: 1000);
        var transfer = Immediate(source, destination, 1000);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, HourlyUsage.None, TestData.Now);

        transfer.Status.Should().Be(TransferStatus.Completed);
        source.Balance.Should().Be(-500);
    }

    [Fact]
    public void Execute_BeyondOverdraft_FailsWithoutMovingMoney()
    {
        var (source, destination) = Parties(sourceBalance: 500, sourceOverdraft: 1000);
        var transfer = Immediate(source, destination, 1600);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, HourlyUsage.None, TestData.Now);

        transfer.Status.Should().Be(TransferStatus.Failed);
        transfer.FailureReason.Should().Be(TransferFailureReason.InsufficientFunds);
        source.Balance.Should().Be(500);
        destination.Balance.Should().Be(0);
    }

    [Fact]
    public void Execute_WithInsufficientFundsWithinLimit_FailsWithInsufficientFunds()
    {
        var (source, destination) = Parties(sourceBalance: 500, sourceOverdraft: 1000);
        var transfer = Immediate(source, destination, 1500.01m);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, HourlyUsage.None, TestData.Now);

        transfer.FailureReason.Should().Be(TransferFailureReason.InsufficientFunds);
        transfer.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TransferFailed>();
        source.Balance.Should().Be(500);
    }

    [Fact]
    public void Execute_ExceedingHourlyAmount_Fails()
    {
        var (source, destination) = Parties(sourceBalance: 10000);
        var transfer = Immediate(source, destination, 1000.01m);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, new HourlyUsage(4000, 1), TestData.Now);

        transfer.FailureReason.Should().Be(TransferFailureReason.HourlyAmountLimitExceeded);
    }

    [Fact]
    public void Execute_ReachingExactHourlyAmount_Completes()
    {
        var (source, destination) = Parties(sourceBalance: 10000);
        var transfer = Immediate(source, destination, 1000);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, new HourlyUsage(4000, 1), TestData.Now);

        transfer.Status.Should().Be(TransferStatus.Completed);
    }

    [Fact]
    public void Execute_AfterMaxAttempts_Fails()
    {
        var (source, destination) = Parties(sourceBalance: 10000);
        var transfer = Immediate(source, destination, 10);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, new HourlyUsage(0, 5), TestData.Now);

        transfer.FailureReason.Should().Be(TransferFailureReason.HourlyAttemptsLimitExceeded);
    }

    [Fact]
    public void Execute_AtNight_AppliesNighttimeLimits()
    {
        var (source, destination) = Parties(sourceBalance: 10000);
        var transfer = Immediate(source, destination, 1500);

        _service.Execute(transfer, source, destination, TransferPeriod.Night, HourlyUsage.None, TestData.Now);

        transfer.FailureReason.Should().Be(TransferFailureReason.HourlyAmountLimitExceeded);
    }

    [Fact]
    public void Execute_AtNightAfterThreeAttempts_Fails()
    {
        var (source, destination) = Parties(sourceBalance: 10000);
        var transfer = Immediate(source, destination, 10);

        _service.Execute(transfer, source, destination, TransferPeriod.Night, new HourlyUsage(0, 3), TestData.Now);

        transfer.FailureReason.Should().Be(TransferFailureReason.HourlyAttemptsLimitExceeded);
    }

    [Fact]
    public void Execute_FromBlockedAccount_Fails()
    {
        var (source, destination) = Parties(sourceBalance: 1000);
        source.Block();
        var transfer = Immediate(source, destination, 10);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, HourlyUsage.None, TestData.Now);

        transfer.FailureReason.Should().Be(TransferFailureReason.SourceAccountInactive);
    }

    [Fact]
    public void Execute_ToInactiveAccount_Fails()
    {
        var (source, destination) = Parties(sourceBalance: 1000);
        destination.Deactivate();
        var transfer = Immediate(source, destination, 10);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, HourlyUsage.None, TestData.Now);

        transfer.FailureReason.Should().Be(TransferFailureReason.DestinationAccountInactive);
        source.Balance.Should().Be(1000);
    }

    [Fact]
    public void Execute_ToDestinationInOverdraft_CoversOverdraftFirst()
    {
        var (source, destination) = Parties(sourceBalance: 5000);
        destination.ChangeOverdraftLimit(1000);
        destination.Debit(800);
        var transfer = Immediate(source, destination, 1000);

        _service.Execute(transfer, source, destination, TransferPeriod.Day, HourlyUsage.None, TestData.Now);

        destination.Balance.Should().Be(200);
        destination.UsedOverdraft.Should().Be(0);
    }

    private static (Account Source, Account Destination) Parties(decimal sourceBalance, decimal sourceOverdraft = 0) =>
        (TestData.Account(sourceBalance, sourceOverdraft, "11111111111"), TestData.Account(document: "22222222222"));

    private static Transfer Immediate(Account source, Account destination, decimal amount) =>
        Transfer.CreateImmediate(source.Id, destination.Id, amount, null, TestData.Now);
}
