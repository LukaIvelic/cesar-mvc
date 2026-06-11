using cesar.Features.Identity;
using cesar.Features.Weather.Entities;
using cesar.Features.Weather.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.Weather;

[ApiController]
[Route("api/weatherforecasts")]
public class WeatherForecastApiController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherForecastApiController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<WeatherForecastDto>>> GetAll(string? q = null)
    {
        var forecasts = await _weatherService.GetAllForecastsAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            forecasts = forecasts.Where(f =>
                f.Date.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || f.TemperatureC.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || (f.Summary?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return Ok(forecasts.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<WeatherForecastDto>> GetById(int id)
    {
        var forecast = await _weatherService.GetForecastByIdAsync(id);
        if (forecast is null)
        {
            return NotFound();
        }

        return Ok(ToDto(forecast));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<WeatherForecastDto>> Create([FromBody] CreateWeatherForecastDto model)
    {
        var forecast = await _weatherService.AddForecastAsync(new WeatherForecast
        {
            Date = model.Date,
            TemperatureC = model.TemperatureC,
            Summary = model.Summary
        });

        return CreatedAtAction(nameof(GetById), new { id = forecast.Id }, ToDto(forecast));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.AdminOrManager)]
    public async Task<ActionResult<WeatherForecastDto>> Update(int id, [FromBody] UpdateWeatherForecastDto model)
    {
        if (model.Id != 0 && model.Id != id)
        {
            return BadRequest(new { error = "Route id and body id must match." });
        }

        var existing = await _weatherService.GetForecastByIdAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        await _weatherService.UpdateForecastAsync(id, model.Date, model.TemperatureC, model.Summary);
        var updated = await _weatherService.GetForecastByIdAsync(id);
        return Ok(ToDto(updated!));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _weatherService.GetForecastByIdAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        await _weatherService.DeleteForecastAsync(id);
        return NoContent();
    }

    private static WeatherForecastDto ToDto(WeatherForecast forecast) =>
        new()
        {
            Id = forecast.Id,
            Date = forecast.Date,
            TemperatureC = forecast.TemperatureC,
            TemperatureF = forecast.TemperatureF,
            Summary = forecast.Summary
        };
}
