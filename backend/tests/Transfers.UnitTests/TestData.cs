using Transfers.Domain.Accounts;
using Transfers.Domain.Limits;

namespace Transfers.UnitTests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 10, 20, 15, 0, 0, TimeSpan.Zero);

    public static TransferLimits DefaultLimits { get; } = TransferLimits.Create(
        HourlyLimit.Create(5000, 5),
        HourlyLimit.Create(1000, 3));

    public static Account Account(decimal balance = 0, decimal overdraftLimit = 0, string document = "00000000000") =>
        Transfers.Domain.Accounts.Account.Open("Titular", document, balance, overdraftLimit, DefaultLimits, Now);
}
