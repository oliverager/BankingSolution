using Banking.Api.Contracts;
using Banking.Core.Entities;
using Banking.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("[Controller]")]
public class AccountsController : ControllerBase
{
    private readonly IRepository<Account> _accounts;
    private readonly IRepository<Customer> _customers;

    public AccountsController(IRepository<Account> accounts, IRepository<Customer> customers)
    {
        _accounts = accounts;
        _customers = customers;
    }

    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null) return BadRequest("CustomerNotFound");

        var account = new Account
        {
            CustomerId = request.CustomerId,
            Iban = request.Iban,
            Balance = request.InitialBalance,
            IsActive = true
        };

        await _accounts.AddAsync(account, ct);
        await _accounts.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Account>> GetAccount(Guid id, CancellationToken ct)
    {
        var account = await _accounts.GetByIdAsync(id, ct);
        if (account is null) return NotFound();
        return account;
    }
}