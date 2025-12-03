using Banking.Core.Entities;
using Banking.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositories;

public class AccountRepository : IRepository<Account>
{
    private readonly BankingDbContext _db;

    public AccountRepository(BankingDbContext db)
    {
        _db = db;
    }

    public Task<IEnumerable<Account>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Accounts
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(Account account, CancellationToken ct = default)
        => await _db.Accounts.AddAsync(account, ct);

    public Task SaveChangesAsync(Account entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(Account entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
    
}