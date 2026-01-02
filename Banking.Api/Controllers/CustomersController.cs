using Banking.Api.Contracts;
using Banking.Core.Entities;
using Banking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("[Controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        try
        {
            var customer = await _customerService.CreateAsync(request.Name, request.Type, ct);
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Customer>> GetCustomer(Guid id, CancellationToken ct)
    {
        var customer = await _customerService.GetByIdAsync(id, ct);
        if (customer is null) return NotFound();
        return customer;
    }
}