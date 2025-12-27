using Banking.Core.Entities;

namespace Banking.Core.Interfaces.Services;

public interface ICollectionService
{
    Task<Collection> GetByIdAsync(Guid collectionId, CancellationToken ct = default);
    Task<Collection> CreateAsync(Guid mandateId, DateTime dueDateUtc, decimal amount, string text, CancellationToken ct = default);
    Task<Collection> CancelAsync(Guid collectionId, CancellationToken ct = default);
    Task<Collection> ApproveAsync(Guid collectionId, CancellationToken ct = default);
    Task<Collection> RejectAsync(Guid collectionId, CancellationToken ct = default);
}