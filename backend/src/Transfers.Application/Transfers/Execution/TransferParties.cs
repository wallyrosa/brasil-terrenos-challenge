using Transfers.Domain.Accounts;

namespace Transfers.Application.Transfers.Execution;

public sealed record TransferParties(Account Source, Account Destination);
