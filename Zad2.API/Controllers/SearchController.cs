using Microsoft.AspNetCore.Mvc;
using Zad2.Core.Interfaces;

namespace Zad2.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string term,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest("Search term is required.");
        }

        var results = await _searchService.SearchAsync(term, limit, cancellationToken);
        return Ok(results);
    }
}