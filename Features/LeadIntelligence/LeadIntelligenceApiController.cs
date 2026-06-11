using cesar.Features.Identity;
using cesar.Features.LeadIntelligence.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.LeadIntelligence;

[ApiController]
[Route("api/leadintelligence")]
public class LeadIntelligenceApiController : ControllerBase
{
    private readonly ILeadIntelligenceService _service;

    public LeadIntelligenceApiController(ILeadIntelligenceService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<LeadIntelligenceDto>>> GetAll(string? q = null)
    {
        var records = await _service.GetAllActiveAsync();
        if (!string.IsNullOrWhiteSpace(q))
        {
            records = records.Where(r =>
                r.Id.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || r.LeadId.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || r.ContentHash.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(records.Select(ToDto));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<LeadIntelligenceDto>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null || entity.ValidTo is not null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpGet("hash/{contentHash}")]
    [AllowAnonymous]
    public async Task<ActionResult<LeadIntelligenceDto>> GetByHash(string contentHash)
    {
        var entity = await _service.GetByContentHashAsync(contentHash);
        if (entity is null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<LeadIntelligenceDto>> Create([FromBody] CreateLeadIntelligenceModel model)
    {
        try
        {
            var entity = await _service.CreateAsync(model.LeadId, model.ContentHash, model.FamiliarityIndex, model.DataDensityScore);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("compute-hash")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public IActionResult ComputeHash([FromBody] string content)
    {
        return Ok(new { hash = _service.ComputeSha256(content) });
    }

    [HttpPost("analyze/{leadId:int}")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<LeadIntelligenceDto>> Analyze(int leadId, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _service.AnalyzeLeadAsync(leadId, cancellationToken);
            return Ok(ToDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<LeadIntelligenceDto>> Update(int id, [FromBody] EditLeadIntelligenceModel model)
    {
        if (model.Id != 0 && model.Id != id)
            return BadRequest(new { error = "Route id and body id must match." });

        var existing = await _service.GetByIdAsync(id);
        if (existing is null || existing.ValidTo is not null)
            return NotFound();

        try
        {
            await _service.UpdateAsync(id, model.LeadId, model.ContentHash, model.FamiliarityIndex, model.DataDensityScore);
            var updated = await _service.GetByIdAsync(id);
            return Ok(ToDto(updated!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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

    private static LeadIntelligenceDto ToDto(Entities.LeadIntelligence entity) =>
        new()
        {
            Id = entity.Id,
            LeadId = entity.LeadId,
            ContentHash = entity.ContentHash,
            FamiliarityIndex = entity.FamiliarityIndex,
            DataDensityScore = entity.DataDensityScore,
            LastAnalyzedAt = entity.LastAnalyzedAt,
            ValidFrom = entity.ValidFrom,
            Lead = entity.Lead is null
                ? null
                : new RawLeadSummaryDto
                {
                    Id = entity.Lead.Id,
                    SourceSystem = entity.Lead.SourceSystem,
                    ExternalId = entity.Lead.ExternalId
                }
        };
}
