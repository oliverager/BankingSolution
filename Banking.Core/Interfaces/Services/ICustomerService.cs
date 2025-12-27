using Banking.Core.Entities;

namespace Banking.Core.Interfaces.Services;

public interface ICustomerService
{
    Task<Customer> CreateAsync(string name, CustomerType type, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
}