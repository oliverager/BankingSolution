using Banking.Core.Transfers;

namespace Banking.Core.Interfaces.Services;

public interface ITransferService
{
    Task<TransferResult> TransferAsync(Guid fromId, Guid toId, decimal amount, CancellationToken ct = default);
}