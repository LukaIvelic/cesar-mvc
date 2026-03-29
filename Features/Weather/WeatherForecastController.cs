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
        var forecasts = await _weatherService.GetAllForecastsAsync();

        var viewModels = forecasts.Select(f => new WeatherForecastViewModel
        {
            Id = f.Id,
            Date = f.Date,
            TemperatureDisplay = $"{f.TemperatureC}°C / {f.TemperatureF}°F",
            Summary = f.Summary ?? "No summary"
        });

        return View(viewModels);
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
            Date = model.Date,
            TemperatureC = model.TemperatureC,
            Summary = model.Summary
        };

        await _weatherService.AddForecastAsync(entity);
        return RedirectToAction(nameof(Index));
    }
}
