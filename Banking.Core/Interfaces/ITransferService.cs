namespace Banking.Core.Transfers;

public interface ITransferService
{
    Task<TransferResult> TransferAsync(Guid fromId, Guid toId, decimal amount, CancellationToken ct = default);
}