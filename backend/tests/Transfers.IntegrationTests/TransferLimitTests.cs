using System.Net;
using FluentAssertions;
using Transfers.Domain.Transfers;
using Transfers.IntegrationTests.Infrastructure;

namespace Transfers.IntegrationTests;

public sealed class TransferLimitTests(TransfersApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Transfer_AboveHourlyAmount_IsRejected()
    {
        var source = await Api.OpenAccountAsync(20000);
        var destination = await Api.OpenAccountAsync(0);
        (await Api.TransferAsync(source.Id, destination.Id, 3000)).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await Api.TransferAsync(source.Id, destination.Id, 2500);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await TransfersApiClient.ReadRejectedTransferAsync(response)).FailureReason
            .Should().Be(TransferFailureReason.HourlyAmountLimitExceeded);
    }

    [Fact]
    public async Task HourlyAmount_IsReleasedAfterOneHour()
    {
        var source = await Api.OpenAccountAsync(20000);
        var destination = await Api.OpenAccountAsync(0);
        await Api.TransferAsync(source.Id, destination.Id, 5000);

        Advance(TimeSpan.FromMinutes(61));
        var response = await Api.TransferAsync(source.Id, destination.Id, 2500);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ExceedingAttempts_CountsRejectedTransfers()
    {
        var source = await Api.OpenAccountAsync(100);
        var destination = await Api.OpenAccountAsync(0);

        (await Api.TransferAsync(source.Id, destination.Id, 1000)).StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        for (var attempt = 0; attempt < 4; attempt++)
            (await Api.TransferAsync(source.Id, destination.Id, 1)).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await Api.TransferAsync(source.Id, destination.Id, 1);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await TransfersApiClient.ReadRejectedTransferAsync(response)).FailureReason
            .Should().Be(TransferFailureReason.HourlyAttemptsLimitExceeded);
        (await Api.GetAccountAsync(source.Id)).Balance.Should().Be(96);
    }

    [Fact]
    public async Task Transfer_AtNight_UsesNighttimeAmountLimit()
    {
        var source = await Api.OpenAccountAsync(20000);
        var destination = await Api.OpenAccountAsync(0);
        MoveToNextBrasiliaTime(new TimeOnly(23, 0));

        var response = await Api.TransferAsync(source.Id, destination.Id, 1500);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await TransfersApiClient.ReadRejectedTransferAsync(response)).FailureReason
            .Should().Be(TransferFailureReason.HourlyAmountLimitExceeded);
        (await Api.TransferAsync(source.Id, destination.Id, 1000)).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Transfer_AtNight_UsesNighttimeAttemptsLimit()
    {
        var source = await Api.OpenAccountAsync(20000);
        var destination = await Api.OpenAccountAsync(0);
        MoveToNextBrasiliaTime(new TimeOnly(2, 0));

        for (var attempt = 0; attempt < 3; attempt++)
            (await Api.TransferAsync(source.Id, destination.Id, 10)).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await Api.TransferAsync(source.Id, destination.Id, 10);

        (await TransfersApiClient.ReadRejectedTransferAsync(response)).FailureReason
            .Should().Be(TransferFailureReason.HourlyAttemptsLimitExceeded);
    }
}
