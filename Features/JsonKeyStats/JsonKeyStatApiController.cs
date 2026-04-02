using cesar.Features.JsonKeyStats.Models;
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
    public async Task<IActionResult> GetAll()
    {
        var stats = await _service.GetAllActiveAsync();
        return Ok(stats.Select(s => new JsonKeyStatViewModel
        {
            Id = s.Id,
            Key = s.Key,
            Occurrences = s.Occurrences,
            ValidFrom = s.ValidFrom
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new JsonKeyStatViewModel { Id = entity.Id, Key = entity.Key, Occurrences = entity.Occurrences, ValidFrom = entity.ValidFrom });
    }

    [HttpGet("key/{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var entity = await _service.GetByKeyAsync(key);
        if (entity is null) return NotFound();

        return Ok(new JsonKeyStatViewModel { Id = entity.Id, Key = entity.Key, Occurrences = entity.Occurrences, ValidFrom = entity.ValidFrom });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJsonKeyStatModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _service.CreateAsync(model.Key, model.Occurrences);
        return Created(string.Empty, new { message = "Key stat created." });
    }

    [HttpPost("track")]
    public async Task<IActionResult> Track([FromBody] IEnumerable<string> keys)
    {
        await _service.TrackKeysAsync(keys);
        return Ok(new { message = "Keys tracked." });
    }

    [HttpPost("increment/{key}")]
    public async Task<IActionResult> Increment(string key)
    {
        await _service.IncrementAsync(key);
        return Ok(new { message = $"'{key}' incremented." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EditJsonKeyStatModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _service.UpdateAsync(id, model.Key, model.Occurrences);
        return Ok(new { message = "Updated." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.SoftDeleteAsync(id);
        return Ok(new { message = "Soft deleted." });
    }
}
