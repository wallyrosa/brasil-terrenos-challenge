using System.Net;
using FluentAssertions;
using Transfers.Domain.Transfers;
using Transfers.IntegrationTests.Infrastructure;

namespace Transfers.IntegrationTests;

public sealed class ImmediateTransferTests(TransfersApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Transfer_WithSufficientBalance_MovesMoney()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);

        var response = await Api.TransferAsync(source.Id, destination.Id, 300);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var transfer = await TransfersApiClient.ReadTransferAsync(response);
        transfer.Status.Should().Be(TransferStatus.Completed);
        (await Api.GetAccountAsync(source.Id)).Balance.Should().Be(700);
        (await Api.GetAccountAsync(destination.Id)).Balance.Should().Be(300);
        (await Api.GetTransferAsync(transfer.Id)).Status.Should().Be(TransferStatus.Completed);
    }

    [Fact]
    public async Task Transfer_UsingOverdraft_LeavesNegativeBalance()
    {
        var source = await Api.OpenAccountAsync(500, overdraftLimit: 1000);
        var destination = await Api.OpenAccountAsync(0);

        var response = await Api.TransferAsync(source.Id, destination.Id, 1000);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var account = await Api.GetAccountAsync(source.Id);
        account.Balance.Should().Be(-500);
        account.UsedOverdraft.Should().Be(500);
        account.AvailableToTransfer.Should().Be(500);
    }

    [Fact]
    public async Task Transfer_BeyondOverdraft_IsRejectedAndPersistedAsFailed()
    {
        var source = await Api.OpenAccountAsync(500, overdraftLimit: 1000);
        var destination = await Api.OpenAccountAsync(0);

        var response = await Api.TransferAsync(source.Id, destination.Id, 1500.01m);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var transfer = await TransfersApiClient.ReadRejectedTransferAsync(response);
        transfer.Status.Should().Be(TransferStatus.Failed);
        transfer.FailureReason.Should().Be(TransferFailureReason.InsufficientFunds);
        (await Api.GetTransferAsync(transfer.Id)).Status.Should().Be(TransferStatus.Failed);
        (await Api.GetAccountAsync(source.Id)).Balance.Should().Be(500);
        (await Api.GetAccountAsync(destination.Id)).Balance.Should().Be(0);
    }

    [Fact]
    public async Task Receiving_WhileInOverdraft_CoversOverdraftFirst()
    {
        var account = await Api.OpenAccountAsync(0, overdraftLimit: 1000);
        var other = await Api.OpenAccountAsync(0);
        var payer = await Api.OpenAccountAsync(5000);
        await Api.TransferAsync(account.Id, other.Id, 800);

        var response = await Api.TransferAsync(payer.Id, account.Id, 1000);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var updated = await Api.GetAccountAsync(account.Id);
        updated.Balance.Should().Be(200);
        updated.UsedOverdraft.Should().Be(0);
    }

    [Fact]
    public async Task Transfer_ToBlockedAccount_IsRejected()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);
        await Api.BlockAsync(destination.Id);

        var response = await Api.TransferAsync(source.Id, destination.Id, 100);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await TransfersApiClient.ReadRejectedTransferAsync(response)).FailureReason
            .Should().Be(TransferFailureReason.DestinationAccountInactive);
    }

    [Fact]
    public async Task Transfer_ToSameAccount_ReturnsUnprocessable()
    {
        var source = await Api.OpenAccountAsync(1000);

        var response = await Api.TransferAsync(source.Id, source.Id, 100);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await TransfersApiClient.ReadErrorCodeAsync(response)).Should().Be("same_source_and_destination");
    }

    [Fact]
    public async Task Transfer_WithZeroAmount_ReturnsUnprocessable()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);

        var response = await Api.TransferAsync(source.Id, destination.Id, 0);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await TransfersApiClient.ReadErrorCodeAsync(response)).Should().Be("amount_must_be_positive");
    }

    [Fact]
    public async Task Transfer_FromUnknownAccount_ReturnsNotFound()
    {
        var destination = await Api.OpenAccountAsync(0);

        var response = await Api.TransferAsync(Guid.NewGuid(), destination.Id, 10);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Transfer_WithoutAmount_ReturnsBadRequest()
    {
        var response = await Api.Http.PostAsync(
            "/api/transfers",
            new StringContent("""{"sourceAccountId":"00000000-0000-0000-0000-000000000001"}""", System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
