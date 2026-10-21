using Transfers.Domain.Accounts;

namespace Transfers.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Account>> LockAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    void Add(Account account);
}
