using Banking.Core.Entities;
using Banking.Core.Interfaces;
using Banking.Core.Interfaces.Repositories;

namespace Banking.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly BankingDbContext _db;

    public TransactionRepository(BankingDbContext db)
    {
        _db = db;
    }

    public Task<IEnumerable<Transaction>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(Transaction transaction, CancellationToken ct = default)
        => await _db.Transactions.AddAsync(transaction, ct);
    
    public Task RemoveAsync(Transaction entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Transaction>> GetForAccountAsync(Guid accountId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}