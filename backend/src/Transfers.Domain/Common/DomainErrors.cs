namespace Transfers.Domain.Common;

public static class DomainErrors
{
    public static DomainException AmountMustBePositive() =>
        new("amount_must_be_positive", "O valor deve ser maior que zero.");

    public static DomainException AmountMustNotBeNegative() =>
        new("amount_must_not_be_negative", "O valor não pode ser negativo.");

    public static DomainException AmountHasInvalidScale(int scale) =>
        new("amount_invalid_scale", $"O valor deve possuir no máximo {scale} casas decimais.");

    public static DomainException HolderNameRequired() =>
        new("holder_name_required", "O nome do titular é obrigatório.");

    public static DomainException DocumentRequired() =>
        new("document_required", "O documento do titular é obrigatório.");

    public static DomainException AccountNotActive(Guid accountId) =>
        new("account_not_active", $"A conta {accountId} não está ativa.");

    public static DomainException InsufficientFunds(Guid accountId) =>
        new("insufficient_funds", $"A conta {accountId} não possui saldo e cheque especial suficientes.");

    public static DomainException AccountAlreadyInStatus(string status) =>
        new("account_already_in_status", $"A conta já está com o status {status}.");

    public static DomainException InvalidHourlyLimit() =>
        new("invalid_hourly_limit", "O limite por hora deve possuir valor e quantidade de tentativas maiores que zero.");

    public static DomainException SameSourceAndDestination() =>
        new("same_source_and_destination", "A conta de origem e a de destino devem ser diferentes.");

    public static DomainException ScheduleMustBeInFuture() =>
        new("schedule_must_be_in_future", "A data de agendamento deve ser futura.");

    public static DomainException InvalidTransferStatusTransition(string from, string to) =>
        new("invalid_transfer_status_transition", $"Não é possível alterar a transferência de {from} para {to}.");

    public static DomainException TransferNotDue() =>
        new("transfer_not_due", "A transferência agendada ainda não atingiu a data de execução.");

    public static DomainException TransferAccountsMismatch() =>
        new("transfer_accounts_mismatch", "As contas informadas não correspondem às contas da transferência.");
}
