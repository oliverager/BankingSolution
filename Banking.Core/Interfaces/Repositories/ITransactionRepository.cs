using Banking.Core.Entities;

namespace Banking.Core.Interfaces.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetForAccountAsync(Guid accountId, CancellationToken ct = default);
}