using cesar.Extensions;
using cesar.Features.JsonKeyStats.Models;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.JsonKeyStats;

public class JsonKeyStatController : Controller
{
    private readonly IJsonKeyStatService _service;

    public JsonKeyStatController(IJsonKeyStatService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        this.SetCurrentPage("Key Stats");
        var stats = await _service.GetAllActiveAsync();
        return View(ToViewModels(stats));
    }

    public async Task<IActionResult> Search(string? q)
    {
        var stats = await _service.GetAllActiveAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            stats = stats.Where(s => s.Key.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return PartialView("_Rows", ToViewModels(stats));
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateJsonKeyStatModel());

    [HttpPost]
    public async Task<IActionResult> Create(CreateJsonKeyStatModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _service.CreateAsync(model.Key, model.Occurrences);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return View(new EditJsonKeyStatModel
        {
            Id = entity.Id,
            Key = entity.Key,
            Occurrences = entity.Occurrences
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditJsonKeyStatModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _service.UpdateAsync(model.Id, model.Key, model.Occurrences);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private static IEnumerable<JsonKeyStatViewModel> ToViewModels(IEnumerable<Entities.JsonKeyStat> stats) =>
        stats.Select(s => new JsonKeyStatViewModel
        {
            Id = s.Id,
            Key = s.Key,
            Occurrences = s.Occurrences,
            ValidFrom = s.ValidFrom
        });
}
