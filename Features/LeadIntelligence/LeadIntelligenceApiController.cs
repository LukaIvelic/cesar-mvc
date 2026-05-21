using cesar.Features.LeadIntelligence.Models;
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
    public async Task<IActionResult> GetAll(string? q = null)
    {
        var records = await _service.GetAllActiveAsync();
        if (!string.IsNullOrWhiteSpace(q))
        {
            records = records.Where(r =>
                r.Id.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || r.LeadId.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || r.ContentHash.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(records.Select(r => new LeadIntelligenceViewModel
        {
            Id = r.Id,
            LeadId = r.LeadId,
            ContentHash = r.ContentHash,
            FamiliarityIndex = r.FamiliarityIndex,
            DataDensityScore = r.DataDensityScore,
            LastAnalyzedAt = r.LastAnalyzedAt,
            ValidFrom = r.ValidFrom
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new LeadIntelligenceViewModel
        {
            Id = entity.Id,
            LeadId = entity.LeadId,
            ContentHash = entity.ContentHash,
            FamiliarityIndex = entity.FamiliarityIndex,
            DataDensityScore = entity.DataDensityScore,
            LastAnalyzedAt = entity.LastAnalyzedAt,
            ValidFrom = entity.ValidFrom
        });
    }

    [HttpGet("hash/{contentHash}")]
    public async Task<IActionResult> GetByHash(string contentHash)
    {
        var entity = await _service.GetByContentHashAsync(contentHash);
        if (entity is null) return NotFound();

        return Ok(new LeadIntelligenceViewModel
        {
            Id = entity.Id,
            LeadId = entity.LeadId,
            ContentHash = entity.ContentHash,
            FamiliarityIndex = entity.FamiliarityIndex,
            DataDensityScore = entity.DataDensityScore,
            LastAnalyzedAt = entity.LastAnalyzedAt,
            ValidFrom = entity.ValidFrom
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadIntelligenceModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _service.CreateAsync(model.LeadId, model.ContentHash, model.FamiliarityIndex, model.DataDensityScore);
            return Created(string.Empty, new { message = "LeadIntelligence record created." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("compute-hash")]
    public IActionResult ComputeHash([FromBody] string content)
    {
        return Ok(new { hash = _service.ComputeSha256(content) });
    }

    [HttpPost("analyze/{leadId:int}")]
    public async Task<IActionResult> Analyze(int leadId, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _service.AnalyzeLeadAsync(leadId, cancellationToken);
            return Ok(new LeadIntelligenceViewModel
            {
                Id = entity.Id,
                LeadId = entity.LeadId,
                ContentHash = entity.ContentHash,
                FamiliarityIndex = entity.FamiliarityIndex,
                DataDensityScore = entity.DataDensityScore,
                LastAnalyzedAt = entity.LastAnalyzedAt,
                ValidFrom = entity.ValidFrom
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EditLeadIntelligenceModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _service.UpdateAsync(id, model.LeadId, model.ContentHash, model.FamiliarityIndex, model.DataDensityScore);
            return Ok(new { message = "Updated." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id);
        return Ok(new { message = "Soft deleted." });
    }
}
