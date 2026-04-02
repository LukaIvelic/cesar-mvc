using cesar.Features.RawLead.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace cesar.Features.RawLead;

public class RawLeadController : Controller
{
    private readonly IRawLeadService _service;

    public RawLeadController(IRawLeadService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var leads = await _service.GetAllActiveAsync();
        var viewModels = leads.Select(l => new RawLeadViewModel
        {
            Id = l.Id,
            SourceSystem = l.SourceSystem,
            ExternalId = l.ExternalId,
            IngestedAt = l.IngestedAt,
            ValidFrom = l.ValidFrom
        });
        return View(viewModels);
    }

    public async Task<IActionResult> Detail(int id)
    {
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
    public IActionResult Create() => View(new CreateRawLeadModel());

    [HttpPost]
    public async Task<IActionResult> Create(CreateRawLeadModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (!IsValidJson(model.RawJsonData))
        {
            ModelState.AddModelError(nameof(model.RawJsonData), "Invalid JSON format.");
            return View(model);
        }

        await _service.CreateAsync(model.SourceSystem, model.ExternalId, model.RawJsonData);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
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
    public async Task<IActionResult> Edit(EditRawLeadModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (!IsValidJson(model.RawJsonData))
        {
            ModelState.AddModelError(nameof(model.RawJsonData), "Invalid JSON format.");
            return View(model);
        }

        await _service.UpdateAsync(model.Id, model.SourceSystem, model.ExternalId, model.RawJsonData);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
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
}
