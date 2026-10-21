using FluentAssertions;
using Microsoft.Extensions.Options;
using Transfers.Application.Limits;
using Transfers.Domain.Limits;

namespace Transfers.UnitTests.Application;

public sealed class TransferPeriodResolverTests
{
    private static readonly TimeSpan BrasiliaOffset = TimeSpan.FromHours(-3);

    private readonly TransferPeriodResolver _resolver = new(Options.Create(new TransferPeriodOptions()));

    [Theory]
    [InlineData(6, 0, TransferPeriod.Day)]
    [InlineData(12, 30, TransferPeriod.Day)]
    [InlineData(21, 59, TransferPeriod.Day)]
    [InlineData(22, 0, TransferPeriod.Night)]
    [InlineData(23, 30, TransferPeriod.Night)]
    [InlineData(0, 0, TransferPeriod.Night)]
    [InlineData(5, 59, TransferPeriod.Night)]
    public void Resolve_UsesBrasiliaLocalTime(int hour, int minute, TransferPeriod expected)
    {
        var instant = new DateTimeOffset(2026, 10, 20, hour, minute, 0, BrasiliaOffset).ToUniversalTime();

        _resolver.Resolve(instant).Should().Be(expected);
    }
}
