using System.Text.Json;
using cesar.Features.DesignTemplates.Models;
using cesar.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.DesignTemplates;

[ApiController]
[Route("api/designtemplates")]
public class DesignTemplateApiController : ControllerBase
{
    private readonly IDesignTemplateService _service;

    public DesignTemplateApiController(IDesignTemplateService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<DesignTemplateDto>>> GetAll(string? q = null)
    {
        var templates = await _service.GetAllActiveAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            templates = templates.Where(t =>
                t.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || t.ContentType.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || t.HtmlMarkup.Contains(q, StringComparison.OrdinalIgnoreCase)
                || t.PlaceholderSchema.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(templates.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<DesignTemplateDto>> GetById(int id)
    {
        var template = await _service.GetByIdAsync(id);
        if (template is null || template.ValidTo is not null)
        {
            return NotFound();
        }

        return Ok(ToDto(template));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<DesignTemplateDto>> Create([FromBody] CreateDesignTemplateDto model)
    {
        if (!IsValidJson(model.PlaceholderSchema))
        {
            return BadRequest(new { error = "PlaceholderSchema is not valid JSON." });
        }

        var template = await _service.CreateAsync(model.Name, model.ContentType, model.HtmlMarkup, model.PlaceholderSchema);
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, ToDto(template));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<DesignTemplateDto>> Update(int id, [FromBody] UpdateDesignTemplateDto model)
    {
        if (model.Id != 0 && model.Id != id)
        {
            return BadRequest(new { error = "Route id and body id must match." });
        }

        var existing = await _service.GetByIdAsync(id);
        if (existing is null || existing.ValidTo is not null)
        {
            return NotFound();
        }

        if (!IsValidJson(model.PlaceholderSchema))
        {
            return BadRequest(new { error = "PlaceholderSchema is not valid JSON." });
        }

        await _service.UpdateAsync(id, model.Name, model.ContentType, model.HtmlMarkup, model.PlaceholderSchema);
        var updated = await _service.GetByIdAsync(id);
        return Ok(ToDto(updated!));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null || existing.ValidTo is not null)
        {
            return NotFound();
        }

        await _service.SoftDeleteAsync(id);
        return NoContent();
    }

    private static DesignTemplateDto ToDto(Entities.DesignTemplate template) =>
        new()
        {
            Id = template.Id,
            Name = template.Name,
            ContentType = template.ContentType,
            HtmlMarkup = template.HtmlMarkup,
            PlaceholderSchema = template.PlaceholderSchema,
            ValidFrom = template.ValidFrom,
            Attachments = template.Attachments
                .OrderByDescending(a => a.CreatedAt)
                .Select(ToAttachmentDto)
                .ToList()
        };

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

    private static bool IsValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
