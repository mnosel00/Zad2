using Microsoft.AspNetCore.Mvc;
using Zad2.Core.Interfaces;

namespace Zad2.API.Controllers;

[ApiController]
[Route("top-pairs")]
public class TopPairsController : ControllerBase
{
    private readonly ITopPairsService _topPairsService;

    public TopPairsController(ITopPairsService topPairsService)
    {
        _topPairsService = topPairsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopPairs(
        [FromQuery] int? min,
        [FromQuery] int? max,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var results = await _topPairsService.GetTopPairsAsync(min, max, limit, cancellationToken);
        return Ok(results);
    }
}