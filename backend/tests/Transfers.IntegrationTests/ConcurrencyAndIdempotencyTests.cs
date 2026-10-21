using System.Net;
using FluentAssertions;
using Transfers.Domain.Transfers;
using Transfers.IntegrationTests.Infrastructure;

namespace Transfers.IntegrationTests;

public sealed class ConcurrencyAndIdempotencyTests(TransfersApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task SimultaneousTransfers_NeverSpendTheSameBalanceTwice()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);
        await Api.ChangeLimitsAsync(source.Id, maxAmountPerHour: 100000, maxAttemptsPerHour: 100);

        var responses = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => Api.TransferAsync(source.Id, destination.Id, 200)));

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).Should().Be(5);
        responses.Count(response => response.StatusCode == HttpStatusCode.UnprocessableEntity).Should().Be(5);
        (await Api.GetAccountAsync(source.Id)).Balance.Should().Be(0);
        (await Api.GetAccountAsync(destination.Id)).Balance.Should().Be(1000);
    }

    [Fact]
    public async Task SimultaneousCrossTransfers_DoNotDeadlock()
    {
        var first = await Api.OpenAccountAsync(1000);
        var second = await Api.OpenAccountAsync(1000);
        await Api.ChangeLimitsAsync(first.Id, maxAmountPerHour: 100000, maxAttemptsPerHour: 100);
        await Api.ChangeLimitsAsync(second.Id, maxAmountPerHour: 100000, maxAttemptsPerHour: 100);

        var responses = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(index => index % 2 == 0
                ? Api.TransferAsync(first.Id, second.Id, 10)
                : Api.TransferAsync(second.Id, first.Id, 10)));

        responses.Should().OnlyContain(response => response.StatusCode == HttpStatusCode.Created);
        (await Api.GetAccountAsync(first.Id)).Balance.Should().Be(1000);
        (await Api.GetAccountAsync(second.Id)).Balance.Should().Be(1000);
    }

    [Fact]
    public async Task RepeatedRequest_WithSameIdempotencyKey_IsAppliedOnce()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);
        var key = Guid.NewGuid().ToString();

        var responses = await Task.WhenAll(Enumerable.Range(0, 3)
            .Select(_ => Api.TransferAsync(source.Id, destination.Id, 100, key)));

        var transfers = await Task.WhenAll(responses.Select(TransfersApiClient.ReadTransferAsync));
        transfers.Select(transfer => transfer.Id).Distinct().Should().ContainSingle();
        transfers.Should().OnlyContain(transfer => transfer.Status == TransferStatus.Completed);
        (await Api.GetAccountAsync(source.Id)).Balance.Should().Be(900);
    }

    [Fact]
    public async Task ReusedIdempotencyKey_WithDifferentPayload_ReturnsConflict()
    {
        var source = await Api.OpenAccountAsync(1000);
        var destination = await Api.OpenAccountAsync(0);
        var key = Guid.NewGuid().ToString();
        await Api.TransferAsync(source.Id, destination.Id, 100, key);

        var response = await Api.TransferAsync(source.Id, destination.Id, 200, key);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await TransfersApiClient.ReadErrorCodeAsync(response)).Should().Be("idempotency_key_reused");
    }
}
