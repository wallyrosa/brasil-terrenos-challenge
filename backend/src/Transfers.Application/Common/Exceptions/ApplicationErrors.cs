namespace Transfers.Application.Common.Exceptions;

public static class ApplicationErrors
{
    public static NotFoundException AccountNotFound(Guid accountId) =>
        new("account_not_found", $"A conta {accountId} não foi encontrada.");

    public static NotFoundException TransferNotFound(Guid transferId) =>
        new("transfer_not_found", $"A transferência {transferId} não foi encontrada.");

    public static ConflictException IdempotencyKeyReused(string idempotencyKey) =>
        new("idempotency_key_reused", $"A chave de idempotência '{idempotencyKey}' já foi utilizada com outros dados.");
}
