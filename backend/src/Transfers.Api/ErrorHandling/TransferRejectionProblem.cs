using Microsoft.AspNetCore.Mvc;
using Transfers.Application.Transfers;
using Transfers.Domain.Transfers;

namespace Transfers.Api.ErrorHandling;

public static class TransferRejectionProblem
{
    public static ProblemDetails From(TransferResponse transfer) =>
        new()
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Transferência rejeitada.",
            Detail = Describe(transfer.FailureReason),
            Extensions =
            {
                ["code"] = transfer.FailureReason?.ToString(),
                ["transfer"] = transfer
            }
        };

    private static string Describe(TransferFailureReason? reason) =>
        reason switch
        {
            TransferFailureReason.SourceAccountInactive => "A conta de origem não está ativa.",
            TransferFailureReason.DestinationAccountInactive => "A conta de destino não está ativa.",
            TransferFailureReason.InsufficientFunds => "Saldo e cheque especial insuficientes.",
            TransferFailureReason.HourlyAmountLimitExceeded => "O valor ultrapassa o limite de transferência da hora.",
            TransferFailureReason.HourlyAttemptsLimitExceeded => "A quantidade de tentativas da hora foi excedida.",
            _ => "A transferência não pôde ser concluída."
        };
}
