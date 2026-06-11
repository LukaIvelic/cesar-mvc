using cesar.Extensions;
using cesar.Features.Identity;
using cesar.Features.LeadIntelligence.Models;
using cesar.Features.RawLead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.LeadIntelligence;

public class LeadIntelligenceController : Controller
{
    private readonly ILeadIntelligenceService _service;
    private readonly IRawLeadService _rawLeadService;

    public LeadIntelligenceController(ILeadIntelligenceService service, IRawLeadService rawLeadService)
    {
        _service = service;
        _rawLeadService = rawLeadService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        this.SetCurrentPage("Intelligence");
        var records = await _service.GetAllActiveAsync();
        return View(ToViewModels(records));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Search(string? q)
    {
        var records = await _service.GetAllActiveAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            records = records.Where(r =>
                r.Id.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || r.LeadId.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || r.ContentHash.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return PartialView("_Rows", ToViewModels(records));
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public IActionResult Create()
    {
        this.SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ("Create", "LeadIntelligence", nameof(Create)));
        return View(new CreateLeadIntelligenceModel());
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Create(CreateLeadIntelligenceModel model)
    {
        await ValidateLeadAsync(model.LeadId);

        if (!ModelState.IsValid)
        {
            this.SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ("Create", "LeadIntelligence", nameof(Create)));
            model.LeadDisplayName = await GetLeadDisplayNameAsync(model.LeadId);
            return View(model);
        }

        await _service.CreateAsync(model.LeadId, model.ContentHash, model.FamiliarityIndex, model.DataDensityScore);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Analyze(CreateLeadIntelligenceModel model, CancellationToken cancellationToken)
    {
        ModelState.Remove(nameof(CreateLeadIntelligenceModel.ContentHash));
        ModelState.Remove(nameof(CreateLeadIntelligenceModel.FamiliarityIndex));
        ModelState.Remove(nameof(CreateLeadIntelligenceModel.DataDensityScore));

        await ValidateLeadAsync(model.LeadId);

        if (!ModelState.IsValid)
        {
            this.SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ("Create", "LeadIntelligence", nameof(Create)));
            model.LeadDisplayName = await GetLeadDisplayNameAsync(model.LeadId);
            return View(nameof(Create), model);
        }

        try
        {
            await _service.AnalyzeLeadAsync(model.LeadId, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            this.SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ("Create", "LeadIntelligence", nameof(Create)));
            model.LeadDisplayName = await GetLeadDisplayNameAsync(model.LeadId);
            return View(nameof(Create), model);
        }
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Edit(int id)
    {
        this.SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ($"Edit #{id}", "LeadIntelligence", nameof(Edit)));
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return View(new EditLeadIntelligenceModel
        {
            Id = entity.Id,
            LeadId = entity.LeadId,
            LeadDisplayName = await GetLeadDisplayNameAsync(entity.LeadId),
            ContentHash = entity.ContentHash,
            FamiliarityIndex = entity.FamiliarityIndex,
            DataDensityScore = entity.DataDensityScore
        });
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Edit(EditLeadIntelligenceModel model)
    {
        await ValidateLeadAsync(model.LeadId);

        if (!ModelState.IsValid)
        {
            this.SetBreadcrumbs(("Intelligence", "LeadIntelligence", nameof(Index)), ($"Edit #{model.Id}", "LeadIntelligence", nameof(Edit)));
            model.LeadDisplayName = await GetLeadDisplayNameAsync(model.LeadId);
            return View(model);
        }

        await _service.UpdateAsync(model.Id, model.LeadId, model.ContentHash, model.FamiliarityIndex, model.DataDensityScore);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateLeadAsync(int leadId)
    {
        if (leadId <= 0)
        {
            return;
        }

        var lead = await _rawLeadService.GetByIdAsync(leadId);
        if (lead is null || lead.ValidTo is not null)
        {
            ModelState.AddModelError(nameof(CreateLeadIntelligenceModel.LeadId), "Select an active raw lead.");
        }
    }

    private async Task<string> GetLeadDisplayNameAsync(int leadId)
    {
        if (leadId <= 0)
        {
            return string.Empty;
        }

        var lead = await _rawLeadService.GetByIdAsync(leadId);
        return lead is null
            ? string.Empty
            : $"#{lead.Id} {lead.SourceSystem} - {lead.ExternalId}".Replace("_", " ");
    }

    private static IEnumerable<LeadIntelligenceViewModel> ToViewModels(IEnumerable<Entities.LeadIntelligence> records) =>
        records.Select(r => new LeadIntelligenceViewModel
        {
            Id = r.Id,
            LeadId = r.LeadId,
            ContentHash = r.ContentHash,
            FamiliarityIndex = r.FamiliarityIndex,
            DataDensityScore = r.DataDensityScore,
            LastAnalyzedAt = r.LastAnalyzedAt,
            ValidFrom = r.ValidFrom
        });
}
