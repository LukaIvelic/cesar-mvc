using cesar.Features.Identity;
using cesar.Features.JsonKeyStats.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.JsonKeyStats;

[ApiController]
[Route("api/jsonkeystats")]
public class JsonKeyStatApiController : ControllerBase
{
    private readonly IJsonKeyStatService _service;

    public JsonKeyStatApiController(IJsonKeyStatService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<JsonKeyStatDto>>> GetAll(string? q = null)
    {
        var stats = await _service.GetAllActiveAsync();
        if (!string.IsNullOrWhiteSpace(q))
        {
            stats = stats.Where(s => s.Key.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(stats.Select(ToDto));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<JsonKeyStatDto>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null || entity.ValidTo is not null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpGet("key/{key}")]
    [AllowAnonymous]
    public async Task<ActionResult<JsonKeyStatDto>> GetByKey(string key)
    {
        var entity = await _service.GetByKeyAsync(key);
        if (entity is null) return NotFound();

        return Ok(ToDto(entity));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<JsonKeyStatDto>> Create([FromBody] CreateJsonKeyStatModel model)
    {
        var entity = await _service.CreateAsync(model.Key, model.Occurrences);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPost("track")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Track([FromBody] IEnumerable<string> keys)
    {
        await _service.TrackKeysAsync(keys);
        return Ok(new { message = "Keys tracked." });
    }

    [HttpPost("increment/{key}")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<IActionResult> Increment(string key)
    {
        await _service.IncrementAsync(key);
        return Ok(new { message = $"'{key}' incremented." });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<JsonKeyStatDto>> Update(int id, [FromBody] EditJsonKeyStatModel model)
    {
        if (model.Id != 0 && model.Id != id)
            return BadRequest(new { error = "Route id and body id must match." });

        var existing = await _service.GetByIdAsync(id);
        if (existing is null || existing.ValidTo is not null)
            return NotFound();

        await _service.UpdateAsync(id, model.Key, model.Occurrences);

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

    private static JsonKeyStatDto ToDto(Entities.JsonKeyStat stat) =>
        new()
        {
            Id = stat.Id,
            Key = stat.Key,
            Occurrences = stat.Occurrences,
            ValidFrom = stat.ValidFrom
        };
}
