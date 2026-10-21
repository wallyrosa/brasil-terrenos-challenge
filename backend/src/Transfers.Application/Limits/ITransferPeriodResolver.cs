using Transfers.Domain.Limits;

namespace Transfers.Application.Limits;

public interface ITransferPeriodResolver
{
    TransferPeriod Resolve(DateTimeOffset instant);
}
