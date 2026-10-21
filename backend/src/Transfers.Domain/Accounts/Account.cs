using Transfers.Domain.Common;
using Transfers.Domain.Limits;

namespace Transfers.Domain.Accounts;

public sealed class Account : AggregateRoot
{
    public string HolderName { get; private set; } = null!;

    public string Document { get; private set; } = null!;

    public decimal Balance { get; private set; }

    public decimal OverdraftLimit { get; private set; }

    public AccountStatus Status { get; private set; }

    public TransferLimits TransferLimits { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    private Account()
    {
    }

    public static Account Open(
        string holderName,
        string document,
        decimal initialBalance,
        decimal overdraftLimit,
        TransferLimits transferLimits,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(holderName))
            throw DomainErrors.HolderNameRequired();

        if (string.IsNullOrWhiteSpace(document))
            throw DomainErrors.DocumentRequired();

        Money.EnsureNonNegative(initialBalance);
        Money.EnsureNonNegative(overdraftLimit);

        return new Account
        {
            Id = Guid.CreateVersion7(),
            HolderName = holderName.Trim(),
            Document = document.Trim(),
            Balance = initialBalance,
            OverdraftLimit = overdraftLimit,
            Status = AccountStatus.Active,
            TransferLimits = transferLimits,
            CreatedAt = now
        };
    }

    public bool IsActive => Status == AccountStatus.Active;

    public decimal UsedOverdraft => Balance < 0 ? -Balance : 0;

    public decimal AvailableOverdraft => Math.Max(0, OverdraftLimit - UsedOverdraft);

    public decimal AvailableToTransfer => Math.Max(0, Balance + OverdraftLimit);

    public bool CanAfford(decimal amount) => amount <= AvailableToTransfer;

    public void Debit(decimal amount)
    {
        EnsureActive();
        Money.EnsurePositive(amount);

        if (!CanAfford(amount))
            throw DomainErrors.InsufficientFunds(Id);

        Balance -= amount;
    }

    public void Credit(decimal amount)
    {
        EnsureActive();
        Money.EnsurePositive(amount);

        Balance += amount;
    }

    public void ChangeOverdraftLimit(decimal overdraftLimit)
    {
        Money.EnsureNonNegative(overdraftLimit);

        OverdraftLimit = overdraftLimit;
    }

    public void ChangeTransferLimits(TransferLimits transferLimits) =>
        TransferLimits = transferLimits;

    public void Activate() => ChangeStatus(AccountStatus.Active);

    public void Block() => ChangeStatus(AccountStatus.Blocked);

    public void Deactivate() => ChangeStatus(AccountStatus.Inactive);

    private void ChangeStatus(AccountStatus status)
    {
        if (Status == status)
            throw DomainErrors.AccountAlreadyInStatus(status.ToString());

        Status = status;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw DomainErrors.AccountNotActive(Id);
    }
}
