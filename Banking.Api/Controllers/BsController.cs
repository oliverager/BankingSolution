using Banking.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("[Controller]")]
public class BsController : ControllerBase
{
    private readonly IBsRunService _bsRunService;

    public BsController(IBsRunService bsRunService)
    {
        _bsRunService = bsRunService;
    }

    // Example: POST /bs/notify?daysAhead=10
    [HttpPost("notify")]
    public async Task<IActionResult> Notify([FromQuery] int daysAhead = 10, CancellationToken ct = default)
    {
        var count = await _bsRunService.NotifyUpcomingAsync(DateTime.UtcNow, daysAhead, ct);
        return Ok(new { notified = count });
    }

    // Example: POST /bs/collect
    [HttpPost("collect")]
    public async Task<IActionResult> Collect(CancellationToken ct = default)
    {
        var count = await _bsRunService.CollectDueAsync(DateTime.UtcNow, ct);
        return Ok(new { collected = count });
    }
}