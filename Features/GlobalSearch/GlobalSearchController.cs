using cesar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.GlobalSearch;

public class GlobalSearchController : Controller
{
    private readonly IGlobalSearchService _searchService;

    public GlobalSearchController(IGlobalSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("/search")]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? q)
    {
        this.SetCurrentPage("Global Search");
        return View(await _searchService.SearchAsync(q, 10));
    }

    [HttpGet("/api/search")]
    [AllowAnonymous]
    public async Task<IActionResult> Api(string? q, int take = 8)
    {
        var safeTake = Math.Clamp(take, 1, 25);
        return Ok(await _searchService.SearchAsync(q, safeTake));
    }
}
