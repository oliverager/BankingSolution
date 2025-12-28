using Banking.Core.Entities;
using Banking.Core.Interfaces.Repositories;
using Banking.Core.Interfaces.Services;

namespace Banking.Core.Services;

public class MandateService : IMandateService
{
    private readonly IMandateRepository _mandates;
    private readonly IRepository<Customer> _customers;
    private readonly IRepository<Account> _accounts;

    public MandateService(
        IMandateRepository mandates,
        IRepository<Customer> customers,
        IRepository<Account> accounts
        )
    {
        _mandates = mandates;
        _customers = customers;
        _accounts = accounts;
        
    }

    public async Task<Mandate> GetByIdAsync(Guid mandateId, CancellationToken ct = default)
        => await _mandates.GetByIdAsync(mandateId, ct)
           ?? throw new InvalidOperationException("MandateNotFound");

    public async Task<Mandate> CreateAsync(Guid debtorCustomerId, Guid payerAccountId,
        Guid settlementAccountId, CancellationToken ct = default)
    {
        if (debtorCustomerId == Guid.Empty) throw new InvalidOperationException("DebtorCustomerIdRequired");
        if (payerAccountId == Guid.Empty) throw new InvalidOperationException("PayerAccountIdRequired");

        // Validate debtor exists
        var debtor = await _customers.GetByIdAsync(debtorCustomerId, ct);
        if (debtor is null)
            throw new InvalidOperationException("DebtorNotFound");

        // Validate payer account exists & active
        var payer = await _accounts.GetByIdAsync(payerAccountId, ct);
        if (payer is null || !payer.IsActive)
            throw new InvalidOperationException("PayerAccountNotFound");

        if (settlementAccountId == Guid.Empty)
            throw new InvalidOperationException("SettlementAccountIdRequired");

        var settlement = await _accounts.GetByIdAsync(settlementAccountId, ct);
        if (settlement is null || !settlement.IsActive)
            throw new InvalidOperationException("SettlementAccountNotFound");


        var mandate = new Mandate
        {
            DebtorCustomerId = debtorCustomerId,
            PayerAccountId = payerAccountId,
            SettlementAccountId = settlementAccountId,
            Status = MandateStatus.Pending,
            CreatedUtc = DateTime.UtcNow
        };


        await _mandates.AddAsync(mandate, ct);
        await _mandates.SaveChangesAsync(ct);

        return mandate;
    }

    public async Task<Mandate> ActivateAsync(Guid mandateId, CancellationToken ct = default)
    {
        var mandate = await GetByIdAsync(mandateId, ct);

        if (mandate.Status == MandateStatus.Cancelled)
            throw new InvalidOperationException("MandateCancelled");

        mandate.Status = MandateStatus.Active;
        mandate.ActivatedUtc = DateTime.UtcNow;

        await _mandates.SaveChangesAsync(ct);
        return mandate;
    }

    public async Task<Mandate> CancelAsync(Guid mandateId, CancellationToken ct = default)
    {
        var mandate = await GetByIdAsync(mandateId, ct);

        mandate.Status = MandateStatus.Cancelled;
        mandate.CancelledUtc = DateTime.UtcNow;

        await _mandates.SaveChangesAsync(ct);
        return mandate;
    }
}