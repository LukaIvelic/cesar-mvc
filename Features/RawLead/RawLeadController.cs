using cesar.Extensions;
using cesar.Features.RawLead.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace cesar.Features.RawLead;

[Route("leads")]
public class RawLeadController : Controller
{
    private readonly IRawLeadService _service;

    public RawLeadController(IRawLeadService service)
    {
        _service = service;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        this.SetCurrentPage("Raw Leads");
        var leads = await _service.GetAllActiveAsync();
        return View(ToViewModels(leads));
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search(string? q)
    {
        var leads = string.IsNullOrWhiteSpace(q)
            ? await _service.GetAllActiveAsync()
            : await _service.SearchActiveAsync(q, 50);

        return PartialView("_Rows", ToViewModels(leads));
    }

    [Route("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        this.SetBreadcrumbs(("Raw Leads", "RawLead", nameof(Index)), ($"#{id}", "RawLead", nameof(Detail)));
        var lead = await _service.GetByIdAsync(id);
        if (lead is null) return NotFound();

        var viewModel = new RawLeadDetailViewModel
        {
            Id = lead.Id,
            SourceSystem = lead.SourceSystem,
            ExternalId = lead.ExternalId,
            RawJsonData = PrettyPrint(lead.RawJsonData),
            IngestedAt = lead.IngestedAt,
            ValidFrom = lead.ValidFrom
        };
        return View(viewModel);
    }

    [HttpGet]
    [Route("create")]
    public IActionResult Create()
    {
        this.SetBreadcrumbs(("Raw Leads", "RawLead", nameof(Index)), ("Create", "RawLead", nameof(Create)));
        return View(new CreateRawLeadModel());
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create(CreateRawLeadModel model)
    {
        if (!ModelState.IsValid)
        {
            this.SetBreadcrumbs(("Raw Leads", "RawLead", nameof(Index)), ("Create", "RawLead", nameof(Create)));
            return View(model);
        }

        if (!IsValidJson(model.RawJsonData))
        {
            ModelState.AddModelError(nameof(model.RawJsonData), "Invalid JSON format.");
            this.SetBreadcrumbs(("Raw Leads", "RawLead", nameof(Index)), ("Create", "RawLead", nameof(Create)));
            return View(model);
        }

        await _service.CreateAsync(model.SourceSystem, model.ExternalId, model.RawJsonData);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Route("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        this.SetBreadcrumbs(("Raw Leads", "RawLead", nameof(Index)), ($"Edit #{id}", "RawLead", nameof(Edit)));
        var lead = await _service.GetByIdAsync(id);
        if (lead is null) return NotFound();

        return View(new EditRawLeadModel
        {
            Id = lead.Id,
            SourceSystem = lead.SourceSystem,
            ExternalId = lead.ExternalId,
            RawJsonData = PrettyPrint(lead.RawJsonData)
        });
    }

    [HttpPost]
    [Route("{id:int}/edit")]
    public async Task<IActionResult> Edit(EditRawLeadModel model)
    {
        if (!ModelState.IsValid)
        {
            this.SetBreadcrumbs(("Raw Leads", "RawLead", nameof(Index)), ($"Edit #{model.Id}", "RawLead", nameof(Edit)));
            return View(model);
        }

        if (!IsValidJson(model.RawJsonData))
        {
            ModelState.AddModelError(nameof(model.RawJsonData), "Invalid JSON format.");
            this.SetBreadcrumbs(("Raw Leads", "RawLead", nameof(Index)), ($"Edit #{model.Id}", "RawLead", nameof(Edit)));
            return View(model);
        }

        await _service.UpdateAsync(model.Id, model.SourceSystem, model.ExternalId, model.RawJsonData);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private static string PrettyPrint(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }

    private static bool IsValidJson(string json)
    {
        try { JsonDocument.Parse(json); return true; }
        catch { return false; }
    }

    private static IEnumerable<RawLeadViewModel> ToViewModels(IEnumerable<Entities.RawLead> leads) =>
        leads.Select(l => new RawLeadViewModel
        {
            Id = l.Id,
            SourceSystem = l.SourceSystem,
            ExternalId = l.ExternalId,
            IngestedAt = l.IngestedAt,
            ValidFrom = l.ValidFrom
        });
}
