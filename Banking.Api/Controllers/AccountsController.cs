using Banking.Api.Contracts;
using Banking.Core.Entities;
using Banking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("[Controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccount([FromBody] CreateAccountRequest request,
        CancellationToken ct)
    {
        try
        {
            var account = await _accountService.CreateAsync(
                request.CustomerId,
                request.Iban,
                request.InitialBalance,
                ct);

            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Account>> GetAccount(Guid id, CancellationToken ct)
    {
        var account = await _accountService.GetByIdAsync(id, ct);
        if (account is null) return NotFound();
        return account;
    }
}