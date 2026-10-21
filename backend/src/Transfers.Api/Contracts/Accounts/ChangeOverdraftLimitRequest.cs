using System.ComponentModel.DataAnnotations;

namespace Transfers.Api.Contracts.Accounts;

public sealed record ChangeOverdraftLimitRequest(
    [Range(0, double.MaxValue)] decimal OverdraftLimit);
