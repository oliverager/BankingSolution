using Banking.Core.Entities;
using Banking.Core.Interfaces;
using Banking.Core.Interfaces.Repositories;
using Banking.Core.Interfaces.Services;

namespace Banking.Core.Services;

public class AccountService : IAccountService
{
    private readonly IRepository<Account> _accounts;
    private readonly IRepository<Customer> _customers;

    public AccountService(IRepository<Account> accounts, IRepository<Customer> customers)
    {
        _accounts = accounts;
        _customers = customers;
    }

    public async Task<Account> CreateAsync(Guid customerId, string iban, decimal initialBalance, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new InvalidOperationException("CustomerIdRequired");

        if (string.IsNullOrWhiteSpace(iban))
            throw new InvalidOperationException("IbanRequired");

        if (initialBalance < 0)
            throw new InvalidOperationException("InvalidInitialBalance");

        var customer = await _customers.GetByIdAsync(customerId, ct);
        if (customer is null)
            throw new InvalidOperationException("CustomerNotFound");

        var account = new Account
        {
            CustomerId = customerId,
            Iban = iban.Trim(),
            Balance = initialBalance,
            IsActive = true
        };

        await _accounts.AddAsync(account, ct);
        await _accounts.SaveChangesAsync(ct);

        return account;
    }

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _accounts.GetByIdAsync(id, ct);
}