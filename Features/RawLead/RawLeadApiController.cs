using cesar.Features.Identity;
using cesar.Features.RawLead.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace cesar.Features.RawLead;

[ApiController]
[Route("api/rawleads")]
public class RawLeadApiController : ControllerBase
{
    private readonly IRawLeadService _service;

    public RawLeadApiController(IRawLeadService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RawLeadDto>>> GetAll(string? q = null)
    {
        var leads = string.IsNullOrWhiteSpace(q)
            ? await _service.GetAllActiveAsync()
            : await _service.SearchActiveAsync(q, 50);

        return Ok(leads.Select(ToDto));
    }

    [HttpGet("autocomplete")]
    [AllowAnonymous]
    public async Task<IActionResult> Autocomplete(string? term = null)
    {
        var leads = await _service.SearchActiveAsync(term ?? string.Empty, 12);
        return Ok(leads.Select(l => new
        {
            id = l.Id,
            text = $"#{l.Id} {l.SourceSystem} - {l.ExternalId}".Replace("_", " ")
        }));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<RawLeadDto>> GetById(int id)
    {
        var lead = await _service.GetByIdAsync(id);
        if (lead is null || lead.ValidTo is not null) return NotFound();

        return Ok(ToDto(lead));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<RawLeadDto>> Create([FromBody] CreateRawLeadModel model)
    {
        if (!IsValidJson(model.RawJsonData))
            return BadRequest(new { error = "RawJsonData is not valid JSON." });

        var entity = await _service.CreateAsync(model.SourceSystem, model.ExternalId, model.RawJsonData);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPost("bulk")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> CreateBulk([FromBody] IEnumerable<CreateRawLeadModel> models)
    {
        var list = models.ToList();
        var invalid = list.Where(m => !IsValidJson(m.RawJsonData)).Select(m => m.ExternalId).ToList();
        if (invalid.Any())
            return BadRequest(new { error = "Invalid JSON in entries.", externalIds = invalid });

        await _service.CreateBulkAsync(list.Select(m => (m.SourceSystem, m.ExternalId, m.RawJsonData)));
        return Created(string.Empty, new { message = $"{list.Count} leads ingested." });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<RawLeadDto>> Update(int id, [FromBody] EditRawLeadModel model)
    {
        if (model.Id != 0 && model.Id != id)
            return BadRequest(new { error = "Route id and body id must match." });

        var existing = await _service.GetByIdAsync(id);
        if (existing is null || existing.ValidTo is not null)
            return NotFound();

        if (!IsValidJson(model.RawJsonData))
            return BadRequest(new { error = "RawJsonData is not valid JSON." });

        await _service.UpdateAsync(id, model.SourceSystem, model.ExternalId, model.RawJsonData);

        var updated = await _service.GetByIdAsync(id);
        return Ok(ToDto(updated!));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null || existing.ValidTo is not null)
            return NotFound();

        await _service.SoftDeleteAsync(id);
        return NoContent();
    }

    private static RawLeadDto ToDto(Entities.RawLead lead) =>
        new()
        {
            Id = lead.Id,
            SourceSystem = lead.SourceSystem,
            ExternalId = lead.ExternalId,
            RawJsonData = lead.RawJsonData,
            IngestedAt = lead.IngestedAt,
            ValidFrom = lead.ValidFrom
        };

    private static bool IsValidJson(string json)
    {
        try { JsonDocument.Parse(json); return true; }
        catch { return false; }
    }
}
