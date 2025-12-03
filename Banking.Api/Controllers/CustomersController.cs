using Banking.Api.Contracts;
using Banking.Core.Entities;
using Banking.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("[Controller]")]
public class CustomersController : ControllerBase
{
    private readonly IRepository<Customer> _customers;

    public CustomersController(IRepository<Customer> customers)
    {
        _customers = customers;
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Type = request.Type
        };

        await _customers.AddAsync(customer, ct);
        await _customers.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Customer>> GetCustomer(Guid id, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(id, ct);
        if (customer is null) return NotFound();
        return customer;
    }
}