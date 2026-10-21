using System.Net;
using FluentAssertions;
using Transfers.Domain.Transfers;
using Transfers.IntegrationTests.Infrastructure;

namespace Transfers.IntegrationTests;

public sealed class ScheduledTransferTests(TransfersApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task ScheduledTransfer_IsExecutedWhenDue()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);

        var response = await Api.ScheduleAsync(source.Id, destination.Id, 400, Now.AddMinutes(30));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var scheduled = await TransfersApiClient.ReadTransferAsync(response);
        scheduled.Status.Should().Be(TransferStatus.Scheduled);
        (await Api.GetAccountAsync(source.Id)).Balance.Should().Be(1000);

        Advance(TimeSpan.FromMinutes(31));
        await ProcessDueScheduledTransfersAsync();

        var executed = await Api.GetTransferAsync(scheduled.Id);
        executed.Status.Should().Be(TransferStatus.Completed);
        (await Api.GetAccountAsync(source.Id)).Balance.Should().Be(600);
        (await Api.GetAccountAsync(destination.Id)).Balance.Should().Be(400);
    }

    [Fact]
    public async Task ScheduledTransfer_IsNotExecutedBeforeDue()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);
        var scheduled = await TransfersApiClient.ReadTransferAsync(
            await Api.ScheduleAsync(source.Id, destination.Id, 400, Now.AddHours(1)));

        Advance(TimeSpan.FromMinutes(30));
        await ProcessDueScheduledTransfersAsync();

        (await Api.GetTransferAsync(scheduled.Id)).Status.Should().Be(TransferStatus.Scheduled);
    }

    [Fact]
    public async Task ScheduledTransfer_WithoutFundsAtExecution_Fails()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);
        var scheduled = await TransfersApiClient.ReadTransferAsync(
            await Api.ScheduleAsync(source.Id, destination.Id, 800, Now.AddMinutes(10)));
        await Api.TransferAsync(source.Id, destination.Id, 500);

        Advance(TimeSpan.FromMinutes(11));
        await ProcessDueScheduledTransfersAsync();

        var executed = await Api.GetTransferAsync(scheduled.Id);
        executed.Status.Should().Be(TransferStatus.Failed);
        executed.FailureReason.Should().Be(TransferFailureReason.InsufficientFunds);
        (await Api.GetAccountAsync(source.Id)).Balance.Should().Be(500);
    }

    [Fact]
    public async Task ScheduledTransfer_ChecksLimitsAgainAtExecution()
    {
        var source = await Api.OpenAccountAsync(20000);
        var destination = await Api.OpenAccountAsync(0);
        var scheduled = await TransfersApiClient.ReadTransferAsync(
            await Api.ScheduleAsync(source.Id, destination.Id, 2000, Now.AddMinutes(10)));
        await Api.TransferAsync(source.Id, destination.Id, 4000);

        Advance(TimeSpan.FromMinutes(11));
        await ProcessDueScheduledTransfersAsync();

        (await Api.GetTransferAsync(scheduled.Id)).FailureReason
            .Should().Be(TransferFailureReason.HourlyAmountLimitExceeded);
    }

    [Fact]
    public async Task Schedule_InThePast_ReturnsUnprocessable()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);

        var response = await Api.ScheduleAsync(source.Id, destination.Id, 100, Now.AddMinutes(-1));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await TransfersApiClient.ReadErrorCodeAsync(response)).Should().Be("schedule_must_be_in_future");
    }

    [Fact]
    public async Task Cancel_ScheduledTransfer_PreventsExecution()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);
        var scheduled = await TransfersApiClient.ReadTransferAsync(
            await Api.ScheduleAsync(source.Id, destination.Id, 400, Now.AddMinutes(10)));

        var response = await Api.CancelAsync(scheduled.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await TransfersApiClient.ReadTransferAsync(response)).Status.Should().Be(TransferStatus.Cancelled);

        Advance(TimeSpan.FromMinutes(11));
        await ProcessDueScheduledTransfersAsync();

        (await Api.GetTransferAsync(scheduled.Id)).Status.Should().Be(TransferStatus.Cancelled);
        (await Api.GetAccountAsync(source.Id)).Balance.Should().Be(1000);
    }

    [Fact]
    public async Task Cancel_AfterExecution_ReturnsUnprocessable()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);
        var scheduled = await TransfersApiClient.ReadTransferAsync(
            await Api.ScheduleAsync(source.Id, destination.Id, 400, Now.AddMinutes(10)));
        Advance(TimeSpan.FromMinutes(11));
        await ProcessDueScheduledTransfersAsync();

        var response = await Api.CancelAsync(scheduled.Id);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await TransfersApiClient.ReadErrorCodeAsync(response)).Should().Be("invalid_transfer_status_transition");
    }
}
