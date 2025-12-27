namespace Banking.Core.Interfaces.Services;

public interface IBsRunService
{
    Task<int> NotifyUpcomingAsync(DateTime nowUtc, int daysAhead, CancellationToken ct = default);
    Task<int> CollectDueAsync(DateTime nowUtc, CancellationToken ct = default);
}