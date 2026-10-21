using Microsoft.EntityFrameworkCore;
using Transfers.Application.Abstractions.Persistence;
using Transfers.Domain.Accounts;

namespace Transfers.Infrastructure.Persistence.Repositories;

internal sealed class AccountRepository(TransfersDbContext context) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Accounts.AsNoTracking().SingleOrDefaultAsync(account => account.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken) =>
        await context.Accounts
            .AsNoTracking()
            .OrderBy(account => account.HolderName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Account>> LockAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        var orderedIds = ids.Distinct().Order().ToArray();

        await context.Database.ExecuteSqlAsync(
            $"SELECT 1 FROM accounts WHERE id = ANY({orderedIds}) ORDER BY id FOR UPDATE",
            cancellationToken);

        return await context.Accounts
            .Where(account => orderedIds.Contains(account.Id))
            .ToListAsync(cancellationToken);
    }

    public void Add(Account account) => context.Accounts.Add(account);
}
