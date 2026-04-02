using cesar.Features.RawLead.Models;
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
    public async Task<IActionResult> GetAll()
    {
        var leads = await _service.GetAllActiveAsync();
        var result = leads.Select(l => new RawLeadViewModel
        {
            Id = l.Id,
            SourceSystem = l.SourceSystem,
            ExternalId = l.ExternalId,
            IngestedAt = l.IngestedAt,
            ValidFrom = l.ValidFrom
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var lead = await _service.GetByIdAsync(id);
        if (lead is null) return NotFound();

        return Ok(new RawLeadDetailViewModel
        {
            Id = lead.Id,
            SourceSystem = lead.SourceSystem,
            ExternalId = lead.ExternalId,
            RawJsonData = lead.RawJsonData,
            IngestedAt = lead.IngestedAt,
            ValidFrom = lead.ValidFrom
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRawLeadModel model)
    {
        if (!IsValidJson(model.RawJsonData))
            return BadRequest(new { error = "RawJsonData is not valid JSON." });

        await _service.CreateAsync(model.SourceSystem, model.ExternalId, model.RawJsonData);
        return Created(string.Empty, new { message = "Lead ingested." });
    }

    [HttpPost("bulk")]
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
    public async Task<IActionResult> Update(int id, [FromBody] EditRawLeadModel model)
    {
        if (!IsValidJson(model.RawJsonData))
            return BadRequest(new { error = "RawJsonData is not valid JSON." });

        await _service.UpdateAsync(id, model.SourceSystem, model.ExternalId, model.RawJsonData);
        return Ok(new { message = "Lead updated." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id);
        return Ok(new { message = "Lead soft deleted." });
    }

    private static bool IsValidJson(string json)
    {
        try { JsonDocument.Parse(json); return true; }
        catch { return false; }
    }
}
