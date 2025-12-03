using Banking.Core.Entities;
using Banking.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositories;

public class CustomerRepository : IRepository<Customer>
{
    private readonly BankingDbContext _db;

    public CustomerRepository(BankingDbContext db)
    {
        _db = db;
    }

    public Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Customers.Include(c => c.Accounts).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
        => await _db.Customers.AddAsync(customer, ct);

    public Task SaveChangesAsync(Customer entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(Customer entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}