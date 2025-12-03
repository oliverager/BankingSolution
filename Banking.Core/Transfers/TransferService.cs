using Banking.Core.Entities;
using Banking.Core.Interfaces;


namespace Banking.Core.Transfers;

public class TransferService : ITransferService
{
    private readonly IRepository<Account> _accounts;
    private readonly IRepository<Transaction> _transactions;

    public TransferService(IRepository<Account> accounts, IRepository<Transaction> transactions)
    {
        _accounts = accounts;
        _transactions = transactions;
    }

    // Decision-table based rule
    public TransferDecision EvaluateLimit(CustomerType type, decimal amount)
    {
        if (amount > 10_000)
            return TransferDecision.Deny("RequiresManualApproval");

        if (type == CustomerType.Standard && amount > 1_000)
            return TransferDecision.Deny("LimitExceeded");

        return TransferDecision.Allow();
    }

    public async Task<TransferResult> TransferAsync(Guid fromId, Guid toId, decimal amount,
        CancellationToken ct = default)
    {
        var from = await _accounts.GetByIdAsync(fromId, ct);
        var to = await _accounts.GetByIdAsync(toId, ct);

        if (from is null || to is null)
            return TransferResult.Fail("AccountNotFound");

        if (!from.IsActive || !to.IsActive)
            return TransferResult.Fail("AccountInactive");

        if (amount <= 0)
            return TransferResult.Fail("InvalidAmount");

        if (from.Balance < amount)
            return TransferResult.Fail("InsufficientBalance");

        if (from.Customer is null)
            return TransferResult.Fail("UnknownCustomer");

        var decision = EvaluateLimit(from.Customer.Type, amount);
        if (!decision.Allowed)
            return TransferResult.Fail(decision.Reason!);

        // Apply transfer
        from.Balance -= amount;
        to.Balance += amount;

        var transaction = new Transaction
        {
            FromAccountId = from.Id,
            ToAccountId = to.Id,
            Amount = amount,
            TimestampUtc = DateTime.UtcNow,
            Status = "Completed"
        };

        await _transactions.AddAsync(transaction, ct);
        await _accounts.SaveChangesAsync(ct);
        await _transactions.SaveChangesAsync(ct);

        return TransferResult.Ok();
    }
}