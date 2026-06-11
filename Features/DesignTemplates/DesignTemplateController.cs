using cesar.Data;
using cesar.Extensions;
using cesar.Features.DesignTemplates.Models;
using cesar.Features.Identity;
using cesar.Features.RawLead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace cesar.Features.DesignTemplates;

public class DesignTemplateController : Controller
{
    private readonly IDesignTemplateService _service;
    private readonly IRawLeadService _rawLeadService;
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public DesignTemplateController(
        IDesignTemplateService service,
        IRawLeadService rawLeadService,
        AppDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _service = service;
        _rawLeadService = rawLeadService;
        _dbContext = dbContext;
        _environment = environment;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        this.SetCurrentPage("Design Templates");
        var templates = await _service.GetAllActiveAsync();
        return View(ToViewModels(templates));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Search(string? q)
    {
        var templates = await _service.GetAllActiveAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            templates = templates.Where(t =>
                t.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || t.ContentType.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || t.PlaceholderSchema.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return PartialView("_Rows", ToViewModels(templates));
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Create()
    {
        this.SetBreadcrumbs(("Design Templates", "DesignTemplate", nameof(Index)), ("Create", "DesignTemplate", nameof(Create)));
        return View(new CreateDesignTemplateModel
        {
            PlaceholderSchema = "{}",
            PreviewRawJsonData = "{}",
            RawJsonOptions = await BuildRawJsonOptionsAsync()
        });
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Create(CreateDesignTemplateModel model)
    {
        if (!ModelState.IsValid)
        {
            this.SetBreadcrumbs(("Design Templates", "DesignTemplate", nameof(Index)), ("Create", "DesignTemplate", nameof(Create)));
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
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Edit(int id)
    {
        this.SetBreadcrumbs(("Design Templates", "DesignTemplate", nameof(Index)), ($"Edit #{id}", "DesignTemplate", nameof(Edit)));
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
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Edit(EditDesignTemplateModel model)
    {
        if (!ModelState.IsValid)
        {
            this.SetBreadcrumbs(("Design Templates", "DesignTemplate", nameof(Index)), ($"Edit #{model.Id}", "DesignTemplate", nameof(Edit)));
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
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> UploadAttachment(int templateId, IFormFile file)
    {
        var template = await _service.GetByIdAsync(templateId);
        if (template is null || template.ValidTo is not null)
        {
            return NotFound();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "Select a file to upload." });
        }

        var uploadsPath = Path.Combine(WebRootPath, "uploads", "design-templates", templateId.ToString());
        Directory.CreateDirectory(uploadsPath);

        var storedFileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var absolutePath = Path.Combine(uploadsPath, storedFileName);

        await using (var stream = new FileStream(absolutePath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new Entities.DesignTemplateAttachment
        {
            DesignTemplateId = templateId,
            FileName = Path.GetFileName(file.FileName),
            FilePath = $"/uploads/design-templates/{templateId}/{storedFileName}",
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            FileSize = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.DesignTemplateAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync();

        return Json(ToAttachmentDto(attachment));
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> GetAttachments(int templateId)
    {
        var attachments = await _dbContext.DesignTemplateAttachments
            .Where(a => a.DesignTemplateId == templateId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return PartialView("_Attachments", attachments.Select(ToAttachmentDto));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var attachment = await _dbContext.DesignTemplateAttachments.FindAsync(id);
        if (attachment is null)
        {
            return NotFound();
        }

        var relativePath = attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(WebRootPath, relativePath);
        if (System.IO.File.Exists(absolutePath))
        {
            System.IO.File.Delete(absolutePath);
        }

        _dbContext.DesignTemplateAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Preview(int id, string? previewRawJsonData = null)
    {
        this.SetBreadcrumbs(("Design Templates", "DesignTemplate", nameof(Index)), ($"Preview #{id}", "DesignTemplate", nameof(Preview)));
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
    [Authorize]
    public async Task<IActionResult> Preview(int id, PreviewDesignTemplateModel model)
    {
        this.SetBreadcrumbs(("Design Templates", "DesignTemplate", nameof(Index)), ($"Preview #{id}", "DesignTemplate", nameof(Preview)));
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
    [Authorize(Roles = AppRoles.AdminOrManager)]
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
                Label = $"#{l.Id} {l.SourceSystem} - {l.ExternalId}".Replace("_", " "),
                RawJsonData = l.RawJsonData
            })
            .ToList();
    }

    private static bool IsValidJson(string json)
    {
        try { JsonDocument.Parse(json); return true; }
        catch { return false; }
    }

    private IEnumerable<DesignTemplateViewModel> ToViewModels(IEnumerable<Entities.DesignTemplate> templates) =>
        templates.Select(t => new DesignTemplateViewModel
        {
            Id = t.Id,
            Name = t.Name,
            ContentType = t.ContentType,
            HtmlMarkup = t.HtmlMarkup,
            PlaceholderSchema = t.PlaceholderSchema,
            PreviewHtml = _service.RenderMarkup(t.HtmlMarkup, t.PlaceholderSchema)
        });

    private string WebRootPath =>
        _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

    private static DesignTemplateAttachmentDto ToAttachmentDto(Entities.DesignTemplateAttachment attachment) =>
        new()
        {
            Id = attachment.Id,
            DesignTemplateId = attachment.DesignTemplateId,
            FileName = attachment.FileName,
            FilePath = attachment.FilePath,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            CreatedAt = attachment.CreatedAt
        };
}
