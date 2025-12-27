using Banking.Api.Contracts;
using Banking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("[Controller]")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;

    public CollectionsController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CollectionResponse>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var c = await _collectionService.GetByIdAsync(id, ct);
            return Ok(c.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpPost]
    public async Task<ActionResult<CollectionResponse>> Create([FromBody] CreateCollectionRequest req,
        CancellationToken ct)
    {
        try
        {
            var c = await _collectionService.CreateAsync(req.MandateId, req.DueDateUtc, req.Amount, req.Text, ct);
            return CreatedAtAction(nameof(GetById), new { id = c.Id }, c.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<CollectionResponse>> Approve(Guid id, CancellationToken ct)
    {
        try
        {
            var c = await _collectionService.ApproveAsync(id, ct);
            return Ok(c.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<CollectionResponse>> Reject(Guid id, CancellationToken ct)
    {
        try
        {
            var c = await _collectionService.RejectAsync(id, ct);
            return Ok(c.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<CollectionResponse>> Cancel(Guid id, CancellationToken ct)
    {
        try
        {
            var c = await _collectionService.CancelAsync(id, ct);
            return Ok(c.ToResponse());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}