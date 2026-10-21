using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transfers.Application.Accounts;
using Transfers.Application.Transfers;

namespace Transfers.IntegrationTests.Infrastructure;

public sealed class TransfersApiClient(HttpClient http)
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public HttpClient Http => http;

    public async Task<AccountResponse> OpenAccountAsync(decimal balance, decimal overdraftLimit = 0)
    {
        var response = await http.PostAsJsonAsync("/api/accounts", new
        {
            holderName = "Titular de Teste",
            document = Guid.NewGuid().ToString("N")[..14],
            initialBalance = balance,
            overdraftLimit
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions))!;
    }

    public async Task<AccountResponse> GetAccountAsync(Guid accountId) =>
        (await http.GetFromJsonAsync<AccountResponse>($"/api/accounts/{accountId}", JsonOptions))!;

    public async Task<AccountResponse> ChangeLimitsAsync(Guid accountId, decimal maxAmountPerHour, int maxAttemptsPerHour)
    {
        var limit = new { maxAmountPerHour, maxAttemptsPerHour };
        var response = await http.PutAsJsonAsync($"/api/accounts/{accountId}/transfer-limits", new { daytime = limit, nighttime = limit });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions))!;
    }

    public Task<HttpResponseMessage> BlockAsync(Guid accountId) =>
        http.PostAsync($"/api/accounts/{accountId}/block", null);

    public Task<HttpResponseMessage> TransferAsync(Guid sourceId, Guid destinationId, decimal amount, string? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/transfers")
        {
            Content = JsonContent.Create(new { sourceAccountId = sourceId, destinationAccountId = destinationId, amount })
        };

        if (idempotencyKey is not null)
            request.Headers.Add("Idempotency-Key", idempotencyKey);

        return http.SendAsync(request);
    }

    public Task<HttpResponseMessage> ScheduleAsync(Guid sourceId, Guid destinationId, decimal amount, DateTimeOffset scheduledFor) =>
        http.PostAsJsonAsync("/api/transfers/scheduled", new
        {
            sourceAccountId = sourceId,
            destinationAccountId = destinationId,
            amount,
            scheduledFor
        });

    public Task<HttpResponseMessage> CancelAsync(Guid transferId) =>
        http.PostAsync($"/api/transfers/{transferId}/cancel", null);

    public async Task<TransferResponse> GetTransferAsync(Guid transferId) =>
        (await http.GetFromJsonAsync<TransferResponse>($"/api/transfers/{transferId}", JsonOptions))!;

    public static async Task<TransferResponse> ReadTransferAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<TransferResponse>(JsonOptions))!;

    public static async Task<TransferResponse> ReadRejectedTransferAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return document.RootElement.GetProperty("transfer").Deserialize<TransferResponse>(JsonOptions)!;
    }

    public static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return document.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }
}
