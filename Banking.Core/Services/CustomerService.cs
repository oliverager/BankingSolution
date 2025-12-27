using Banking.Core.Entities;
using Banking.Core.Interfaces;
using Banking.Core.Interfaces.Repositories;
using Banking.Core.Interfaces.Services;

namespace Banking.Core.Services;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customers;

    public CustomerService(IRepository<Customer> customers)
    {
        _customers = customers;
    }

    public async Task<Customer> CreateAsync(string name, CustomerType type, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("NameRequired");

        var customer = new Customer
        {
            Name = name.Trim(),
            Type = type
        };

        await _customers.AddAsync(customer, ct);
        await _customers.SaveChangesAsync(ct);

        return customer;
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _customers.GetByIdAsync(id, ct);
}