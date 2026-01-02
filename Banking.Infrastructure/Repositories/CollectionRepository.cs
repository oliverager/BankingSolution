using Banking.Core.Entities;
using Banking.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly BankingDbContext _db;

    public CollectionRepository(BankingDbContext db)
    {
        _db = db;
    }

    public Task<Collection?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Collections.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task AddAsync(Collection entity, CancellationToken ct = default)
        => _db.Collections.AddAsync(entity, ct).AsTask();

    public Task<List<Collection>> GetUpcomingForNotificationAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
        => _db.Collections
            .Where(c =>
                c.Status == CollectionStatus.Created &&
                c.DueDateUtc >= fromUtc.Date &&
                c.DueDateUtc <= toUtc.Date)
            .ToListAsync(ct);

    public Task<List<Collection>> GetDueForCollectionAsync(DateTime nowUtc, CancellationToken ct = default)
        => _db.Collections
            .Where(c =>
                (c.Status == CollectionStatus.Created ||
                 c.Status == CollectionStatus.Notified ||
                 c.Status == CollectionStatus.Approved) &&
                c.DueDateUtc <= nowUtc.Date)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}