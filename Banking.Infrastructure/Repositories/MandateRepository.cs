using Banking.Core.Entities;
using Banking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositories;

public class MandateRepository : IMandateRepository
{
    private readonly BankingDbContext _db;

    public MandateRepository(BankingDbContext db)
    {
        _db = db;
    }

    public Task<Mandate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Mandates.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task AddAsync(Mandate mandate, CancellationToken ct = default)
        => _db.Mandates.AddAsync(mandate, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}