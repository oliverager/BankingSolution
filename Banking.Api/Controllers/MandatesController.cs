using Banking.Api.Contracts;
using Banking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MandatesController : ControllerBase
{
    private readonly IMandateService _mandates;

    public MandatesController(IMandateService mandates)
    {
        _mandates = mandates;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MandateResponse>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var m = await _mandates.GetByIdAsync(id, ct);
            return Ok(m.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<MandateResponse>> Create([FromBody] CreateMandateRequest req, CancellationToken ct)
    {
        try
        {
            var m = await _mandates.CreateAsync(req.DebtorCustomerId, req.PayerAccountId, req.SettlementAccountId, ct);
            return CreatedAtAction(nameof(GetById), new { id = m.Id }, m.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<MandateResponse>> Activate(Guid id, CancellationToken ct)
    {
        try
        {
            var m = await _mandates.ActivateAsync(id, ct);
            return Ok(m.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<MandateResponse>> Cancel(Guid id, CancellationToken ct)
    {
        try
        {
            var m = await _mandates.CancelAsync(id, ct);
            return Ok(m.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}