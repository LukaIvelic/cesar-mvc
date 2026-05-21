using cesar.Extensions;
using cesar.Features.Weather.Entities;
using cesar.Features.Weather.Models;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.Weather;

public class WeatherForecastController : Controller
{
    private readonly IWeatherService _weatherService;

    public WeatherForecastController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public async Task<IActionResult> Index()
    {
        this.SetCurrentPage("Weather");
        var forecasts = await _weatherService.GetAllForecastsAsync();
        return View(ToViewModels(forecasts));
    }

    public async Task<IActionResult> Search(string? q)
    {
        var forecasts = await _weatherService.GetAllForecastsAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            forecasts = forecasts.Where(f =>
                f.Date.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || f.TemperatureC.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
                || (f.Summary?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return PartialView("_Rows", ToViewModels(forecasts));
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateWeatherForecastModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWeatherForecastModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var entity = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(model.Date),
            TemperatureC = model.TemperatureC,
            Summary = model.Summary
        };

        await _weatherService.AddForecastAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var forecast = await _weatherService.GetForecastByIdAsync(id);
        if (forecast is null) return NotFound();

        return View(new EditWeatherForecastModel
        {
            Id = forecast.Id,
            Date = forecast.Date.ToDateTime(TimeOnly.MinValue),
            TemperatureC = forecast.TemperatureC,
            Summary = forecast.Summary
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditWeatherForecastModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _weatherService.UpdateForecastAsync(
            model.Id,
            DateOnly.FromDateTime(model.Date),
            model.TemperatureC,
            model.Summary);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _weatherService.DeleteForecastAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private static IEnumerable<WeatherForecastViewModel> ToViewModels(IEnumerable<WeatherForecast> forecasts) =>
        forecasts.Select(f => new WeatherForecastViewModel
        {
            Id = f.Id,
            Date = f.Date,
            TemperatureDisplay = $"{f.TemperatureC}C / {f.TemperatureF}F",
            Summary = f.Summary ?? "No summary"
        });
}
