using cesar.Features.DesignTemplates.Models;
using cesar.Features.RawLead;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace cesar.Features.DesignTemplates;

public class DesignTemplateController : Controller
{
    private readonly IDesignTemplateService _service;
    private readonly IRawLeadService _rawLeadService;

    public DesignTemplateController(IDesignTemplateService service, IRawLeadService rawLeadService)
    {
        _service = service;
        _rawLeadService = rawLeadService;
    }

    public async Task<IActionResult> Index()
    {
        var templates = await _service.GetAllActiveAsync();
        var viewModels = templates.Select(t => new DesignTemplateViewModel
        {
            Id = t.Id,
            Name = t.Name,
            ContentType = t.ContentType,
            HtmlMarkup = t.HtmlMarkup,
            PlaceholderSchema = t.PlaceholderSchema,
            PreviewHtml = _service.RenderMarkup(t.HtmlMarkup, t.PlaceholderSchema)
        });

        return View(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new CreateDesignTemplateModel
        {
            PlaceholderSchema = "{}",
            PreviewRawJsonData = "{}",
            RawJsonOptions = await BuildRawJsonOptionsAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDesignTemplateModel model)
    {
        if (!ModelState.IsValid)
        {
            model.RawJsonOptions = await BuildRawJsonOptionsAsync();
            return View(model);
        }

        if (!IsValidJson(model.PlaceholderSchema))
        {
            ModelState.AddModelError(nameof(model.PlaceholderSchema), "Placeholder schema must be valid JSON.");
        }

        if (!IsValidJson(model.PreviewRawJsonData))
        {
            ModelState.AddModelError(nameof(model.PreviewRawJsonData), "Preview raw JSON must be valid JSON.");
        }

        if (!ModelState.IsValid)
        {
            model.RawJsonOptions = await BuildRawJsonOptionsAsync();
            return View(model);
        }

        await _service.CreateAsync(model.Name, model.ContentType, model.HtmlMarkup, model.PlaceholderSchema);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var template = await _service.GetByIdAsync(id);
        if (template is null) return NotFound();

        return View(new EditDesignTemplateModel
        {
            Id = template.Id,
            Name = template.Name,
            ContentType = template.ContentType,
            HtmlMarkup = template.HtmlMarkup,
            PlaceholderSchema = template.PlaceholderSchema,
            PreviewRawJsonData = template.PlaceholderSchema,
            RawJsonOptions = await BuildRawJsonOptionsAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditDesignTemplateModel model)
    {
        if (!ModelState.IsValid)
        {
            model.RawJsonOptions = await BuildRawJsonOptionsAsync();
            return View(model);
        }

        if (!IsValidJson(model.PlaceholderSchema))
        {
            ModelState.AddModelError(nameof(model.PlaceholderSchema), "Placeholder schema must be valid JSON.");
        }

        if (!IsValidJson(model.PreviewRawJsonData))
        {
            ModelState.AddModelError(nameof(model.PreviewRawJsonData), "Preview raw JSON must be valid JSON.");
        }

        if (!ModelState.IsValid)
        {
            model.RawJsonOptions = await BuildRawJsonOptionsAsync();
            return View(model);
        }

        await _service.UpdateAsync(model.Id, model.Name, model.ContentType, model.HtmlMarkup, model.PlaceholderSchema);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Preview(int id, string? previewRawJsonData = null)
    {
        var template = await _service.GetByIdAsync(id);
        if (template is null) return NotFound();

        var rawJsonData = string.IsNullOrWhiteSpace(previewRawJsonData)
            ? template.PlaceholderSchema
            : previewRawJsonData;

        var model = new PreviewDesignTemplateModel
        {
            Id = template.Id,
            Name = template.Name,
            ContentType = template.ContentType,
            HtmlMarkup = template.HtmlMarkup,
            PlaceholderSchema = template.PlaceholderSchema,
            PreviewRawJsonData = rawJsonData,
            RenderedHtml = _service.RenderMarkup(template.HtmlMarkup, rawJsonData),
            RawJsonOptions = await BuildRawJsonOptionsAsync()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Preview(int id, PreviewDesignTemplateModel model)
    {
        var template = await _service.GetByIdAsync(id);
        if (template is null) return NotFound();

        if (!IsValidJson(model.PreviewRawJsonData))
        {
            ModelState.AddModelError(nameof(model.PreviewRawJsonData), "Preview raw JSON must be valid JSON.");
        }

        model.Id = template.Id;
        model.Name = template.Name;
        model.ContentType = template.ContentType;
        model.HtmlMarkup = template.HtmlMarkup;
        model.PlaceholderSchema = template.PlaceholderSchema;
        model.RenderedHtml = _service.RenderMarkup(template.HtmlMarkup, model.PreviewRawJsonData);
        model.RawJsonOptions = await BuildRawJsonOptionsAsync();

        return View(model);
    }

    [HttpPost]
    public IActionResult PreviewDraft(string htmlMarkup, string previewRawJsonData)
    {
        var rendered = IsValidJson(previewRawJsonData)
            ? _service.RenderMarkup(htmlMarkup, previewRawJsonData)
            : htmlMarkup;

        return Content(rendered, "text/html");
    }

    private async Task<List<RawJsonOptionModel>> BuildRawJsonOptionsAsync()
    {
        var leads = await _rawLeadService.GetAllActiveAsync();
        return leads
            .OrderByDescending(l => l.IngestedAt)
            .Select(l => new RawJsonOptionModel
            {
                Id = l.Id,
                Label = $"#{l.Id} — {l.SourceSystem} — {l.ExternalId}",
                RawJsonData = l.RawJsonData
            })
            .ToList();
    }

    private static bool IsValidJson(string json)
    {
        try { JsonDocument.Parse(json); return true; }
        catch { return false; }
    }
}
