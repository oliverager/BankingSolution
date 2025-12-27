using Banking.Core.Entities;

namespace Banking.Core.Interfaces.Services;

public interface IMandateService
{
    Task<Mandate> GetByIdAsync(Guid mandateId, CancellationToken ct = default);
    Task<Mandate> CreateAsync(Guid debtorCustomerId, Guid payerAccountId, Guid creditorId, Guid settlementAccountId, CancellationToken ct = default);
    Task<Mandate> ActivateAsync(Guid mandateId, CancellationToken ct = default);
    Task<Mandate> CancelAsync(Guid mandateId, CancellationToken ct = default);
}