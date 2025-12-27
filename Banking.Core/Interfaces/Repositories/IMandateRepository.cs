using Banking.Core.Entities;

namespace Banking.Core.Interfaces.Repositories;

public interface IMandateRepository
{
    Task<Mandate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Mandate mandate, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}