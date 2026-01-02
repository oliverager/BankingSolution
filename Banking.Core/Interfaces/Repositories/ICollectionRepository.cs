using Banking.Core.Entities;

namespace Banking.Core.Interfaces.Repositories;

public interface ICollectionRepository
{
    Task<Collection?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Collection entity, CancellationToken ct = default);
    Task<List<Collection>> GetUpcomingForNotificationAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<List<Collection>> GetDueForCollectionAsync(DateTime nowUtc, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}