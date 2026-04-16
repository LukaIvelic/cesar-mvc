using cesar.Extensions;
using cesar.Features.LeadIntelligence.Models;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.LeadIntelligence;

public class LeadIntelligenceController : Controller
{
    private readonly ILeadIntelligenceService _service;

    public LeadIntelligenceController(ILeadIntelligenceService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        SetCurrentPage("Intelligence");
        var records = await _service.GetAllActiveAsync();
        var viewModels = records.Select(r => new LeadIntelligenceViewModel
        {
            Id = r.Id,
            LeadId = r.LeadId,
            ContentHash = r.ContentHash,
            FamiliarityIndex = r.FamiliarityIndex,
            DataDensityScore = r.DataDensityScore,
            LastAnalyzedAt = r.LastAnalyzedAt,
            ValidFrom = r.ValidFrom
        });
        return View(viewModels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ("Create", "LeadIntelligence", nameof(Create)));
        return View(new CreateLeadIntelligenceModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLeadIntelligenceModel model)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ("Create", "LeadIntelligence", nameof(Create)));
            return View(model);
        }

        await _service.CreateAsync(model.LeadId, model.ContentHash, model.FamiliarityIndex, model.DataDensityScore);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ($"Edit #{id}", "LeadIntelligence", nameof(Edit)));
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return View(new EditLeadIntelligenceModel
        {
            Id = entity.Id,
            LeadId = entity.LeadId,
            ContentHash = entity.ContentHash,
            FamiliarityIndex = entity.FamiliarityIndex,
            DataDensityScore = entity.DataDensityScore
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditLeadIntelligenceModel model)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ($"Edit #{model.Id}", "LeadIntelligence", nameof(Edit)));
            return View(model);
        }

        await _service.UpdateAsync(model.Id, model.LeadId, model.ContentHash, model.FamiliarityIndex, model.DataDensityScore);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
