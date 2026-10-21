using System.ComponentModel.DataAnnotations;
using Transfers.Application.Accounts.OpenAccount;

namespace Transfers.Api.Contracts.Accounts;

public sealed record OpenAccountRequest(
    [Required, MaxLength(200)] string HolderName,
    [Required, MaxLength(20)] string Document,
    [Range(0, double.MaxValue)] decimal InitialBalance,
    [Range(0, double.MaxValue)] decimal OverdraftLimit)
{
    public OpenAccountCommand ToCommand() =>
        new(HolderName, Document, InitialBalance, OverdraftLimit);
}
