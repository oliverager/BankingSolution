using Banking.Core.Entities;
using Banking.Core.Interfaces.Repositories;
using Banking.Core.Interfaces.Services;

namespace Banking.Core.Services;

public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository _collections;
    private readonly IMandateRepository _mandates;

    public CollectionService(ICollectionRepository collections, IMandateRepository mandates)
    {
        _collections = collections;
        _mandates = mandates;
    }

    public async Task<Collection> GetByIdAsync(Guid collectionId, CancellationToken ct = default)
        => await _collections.GetByIdAsync(collectionId, ct)
           ?? throw new InvalidOperationException("CollectionNotFound");

    public async Task<Collection> CreateAsync(Guid mandateId, DateTime dueDateUtc, decimal amount, string text,
        CancellationToken ct = default)
    {
        if (amount <= 0) throw new InvalidOperationException("InvalidAmount");

        var mandate = await _mandates.GetByIdAsync(mandateId, ct)
                      ?? throw new InvalidOperationException("MandateNotFound");

        if (mandate.Status != MandateStatus.Active)
            throw new InvalidOperationException("MandateNotActive");

        var today = DateTime.UtcNow.Date;
        if (dueDateUtc.Date < today) throw new InvalidOperationException("DueDateInPast");

        var collection = new Collection
        {
            MandateId = mandateId,
            DueDateUtc = dueDateUtc.Date,
            Amount = amount,
            Text = text ?? string.Empty,
            Status = CollectionStatus.Created
        };

        await _collections.AddAsync(collection, ct);
        await _collections.SaveChangesAsync(ct);

        return collection;
    }

    public async Task<Collection> CancelAsync(Guid collectionId, CancellationToken ct = default)
    {
        var c = await _collections.GetByIdAsync(collectionId, ct)
                ?? throw new InvalidOperationException("CollectionNotFound");

        if (c.Status is CollectionStatus.Collected)
            throw new InvalidOperationException("AlreadyCollected");

        c.Status = CollectionStatus.Cancelled;
        await _collections.SaveChangesAsync(ct);
        return c;
    }

    public async Task<Collection> ApproveAsync(Guid collectionId, CancellationToken ct = default)
    {
        var c = await _collections.GetByIdAsync(collectionId, ct)
                ?? throw new InvalidOperationException("CollectionNotFound");

        if (c.Status is CollectionStatus.Cancelled or CollectionStatus.Collected)
            throw new InvalidOperationException("InvalidStatus");

        c.Status = CollectionStatus.Approved;
        c.DecisionUtc = DateTime.UtcNow;

        await _collections.SaveChangesAsync(ct);
        return c;
    }

    public async Task<Collection> RejectAsync(Guid collectionId, CancellationToken ct = default)
    {
        var c = await _collections.GetByIdAsync(collectionId, ct)
                ?? throw new InvalidOperationException("CollectionNotFound");

        if (c.Status is CollectionStatus.Cancelled or CollectionStatus.Collected)
            throw new InvalidOperationException("InvalidStatus");

        c.Status = CollectionStatus.Rejected;
        c.DecisionUtc = DateTime.UtcNow;

        await _collections.SaveChangesAsync(ct);
        return c;
    }
}