namespace Transfers.Application.Accounts.OpenAccount;

public sealed record OpenAccountCommand(
    string HolderName,
    string Document,
    decimal InitialBalance,
    decimal OverdraftLimit);
