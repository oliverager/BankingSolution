using Banking.Core.Entities;

namespace Banking.Core.Interfaces.Services;

public interface IAccountService
{
    Task<Account> CreateAsync(Guid customerId, string iban, decimal initialBalance, CancellationToken ct = default);
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
}