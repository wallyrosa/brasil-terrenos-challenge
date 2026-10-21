using FluentAssertions;
using Transfers.Domain.Common;

namespace Transfers.UnitTests.Domain;

public sealed class AccountTests
{
    [Fact]
    public void Debit_WithSufficientBalance_DecreasesBalance()
    {
        var account = TestData.Account(balance: 1000);

        account.Debit(400);

        account.Balance.Should().Be(600);
        account.UsedOverdraft.Should().Be(0);
    }

    [Fact]
    public void AvailableToTransfer_IsBalancePlusOverdraftLimit()
    {
        var account = TestData.Account(balance: 500, overdraftLimit: 1000);

        account.AvailableToTransfer.Should().Be(1500);
    }

    [Fact]
    public void Debit_BeyondBalance_UsesOverdraft()
    {
        var account = TestData.Account(balance: 500, overdraftLimit: 1000);

        account.Debit(1000);

        account.Balance.Should().Be(-500);
        account.UsedOverdraft.Should().Be(500);
        account.AvailableOverdraft.Should().Be(500);
        account.AvailableToTransfer.Should().Be(500);
    }

    [Fact]
    public void Debit_BeyondOverdraftLimit_Throws()
    {
        var account = TestData.Account(balance: 500, overdraftLimit: 1000);

        var act = () => account.Debit(1500.01m);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("insufficient_funds");
        account.Balance.Should().Be(500);
    }

    [Fact]
    public void Credit_WhileUsingOverdraft_CoversOverdraftFirst()
    {
        var account = TestData.Account(balance: 200, overdraftLimit: 1000);
        account.Debit(1000);

        account.Credit(1000);

        account.Balance.Should().Be(200);
        account.UsedOverdraft.Should().Be(0);
        account.AvailableOverdraft.Should().Be(1000);
    }

    [Fact]
    public void Credit_PartiallyCoveringOverdraft_KeepsNegativeBalance()
    {
        var account = TestData.Account(balance: 0, overdraftLimit: 1000);
        account.Debit(800);

        account.Credit(300);

        account.Balance.Should().Be(-500);
        account.UsedOverdraft.Should().Be(500);
    }

    [Fact]
    public void Debit_OnBlockedAccount_Throws()
    {
        var account = TestData.Account(balance: 1000);
        account.Block();

        var act = () => account.Debit(100);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("account_not_active");
    }

    [Fact]
    public void Credit_OnInactiveAccount_Throws()
    {
        var account = TestData.Account(balance: 1000);
        account.Deactivate();

        var act = () => account.Credit(100);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("account_not_active");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(10.001)]
    public void Debit_WithInvalidAmount_Throws(decimal amount)
    {
        var account = TestData.Account(balance: 1000);

        var act = () => account.Debit(amount);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeOverdraftLimit_WithNegativeValue_Throws()
    {
        var account = TestData.Account();

        var act = () => account.ChangeOverdraftLimit(-1);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("amount_must_not_be_negative");
    }

    [Fact]
    public void Block_WhenAlreadyBlocked_Throws()
    {
        var account = TestData.Account();
        account.Block();

        var act = () => account.Block();

        act.Should().Throw<DomainException>().Which.Code.Should().Be("account_already_in_status");
    }
}
