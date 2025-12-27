using Banking.Api.Contracts;
using Banking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("[Controller]")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    [HttpPost]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request, CancellationToken ct)
    {
        var result = await _transferService.TransferAsync(
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            ct);

        if (!result.Success)
            return BadRequest(new { error = result.Reason });

        return Ok(new { success = true });
    }
}