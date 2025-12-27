using Banking.Core.Entities;
using Banking.Core.Interfaces.Repositories;
using Banking.Core.Interfaces.Services;

namespace Banking.Core.Services;

public class BsRunService : IBsRunService
{
    private readonly ICollectionRepository _collections;
    private readonly IMandateRepository _mandates;
    private readonly ITransferService _transferService;

    public BsRunService(
        ICollectionRepository collections,
        IMandateRepository mandates,
        ITransferService transferService)
    {
        _collections = collections;
        _mandates = mandates;
        _transferService = transferService;
    }

    public async Task<int> NotifyUpcomingAsync(DateTime nowUtc, int daysAhead, CancellationToken ct = default)
    {
        var toUtc = nowUtc.Date.AddDays(daysAhead);

        var toNotify = await _collections.GetUpcomingForNotificationAsync(nowUtc, toUtc, ct);

        foreach (var c in toNotify)
        {
            c.Status = CollectionStatus.Notified;
            c.NotifiedUtc = nowUtc;
        }

        if (toNotify.Count > 0)
            await _collections.SaveChangesAsync(ct);

        return toNotify.Count;
    }

    public async Task<int> CollectDueAsync(DateTime nowUtc, CancellationToken ct = default)
    {
        var due = await _collections.GetDueForCollectionAsync(nowUtc, ct);

        var processed = 0;

        foreach (var c in due)
        {
            var mandate = await _mandates.GetByIdAsync(c.MandateId, ct);
            if (mandate is null)
            {
                c.Status = CollectionStatus.Failed;
                c.FailureReason = "MandateNotFound";
                continue;
            }

            if (mandate.Status != MandateStatus.Active)
            {
                c.Status = CollectionStatus.Failed;
                c.FailureReason = "MandateNotActive";
                continue;
            }
            
            if (mandate.SettlementAccountId == Guid.Empty)
            {
                c.Status = CollectionStatus.Failed;
                c.FailureReason = "SettlementAccountMissing";
                continue;
            }

            var result = await _transferService.TransferAsync(
                mandate.PayerAccountId,
                mandate.SettlementAccountId,
                c.Amount,
                ct);

            if (result.Success)
            {
                c.Status = CollectionStatus.Collected;
                c.CollectedUtc = nowUtc;
                c.FailureReason = null;
                processed++;
            }
            else
            {
                c.Status = CollectionStatus.Failed;
                c.FailureReason = result.Reason ?? "TransferFailed";
            }
        }

        if (due.Count > 0)
            await _collections.SaveChangesAsync(ct);

        return processed;
    }
}