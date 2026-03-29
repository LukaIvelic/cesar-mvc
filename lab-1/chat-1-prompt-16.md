# Chat 1 - Prompt 16

## User

Okay now tell me how exactly should I display one row from backend on frontend? Where should I fetch the data and how exactly? I create a WeatherForecast controller right?

---

## Claude

Yes, you create a `WeatherForecastController`. Here's the full flow:

**Request → Controller → Service → View**

The controller fetches data from the service and passes it to the view. That's the only job of a controller action.

---

**1. Create the controller** — `Controllers/WeatherForecastController.cs`

```csharp
using cesar.Services;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Controllers;

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
        return View(forecasts);
    }
}
```

**2. Create the view** — `Views/WeatherForecast/Index.cshtml`
The folder name must match the controller name (without "Controller"), the file name must match the action name. ASP.NET finds it automatically.

```html
@model IEnumerable<cesar.Entities.WeatherForecast>

<h2>Weather Forecasts</h2>

@foreach (var forecast in Model)
{
    <div>
        <p>Date: @forecast.Date</p>
        <p>Temp: @forecast.TemperatureC °C / @forecast.TemperatureF °F</p>
        <p>Summary: @forecast.Summary</p>
    </div>
}
```

**3. Hit the URL** — `https://localhost:{port}/WeatherForecast`

---

The `@model` line at the top of the view declares what type of data the view expects — it must match exactly what `return View(forecasts)` passes from the controller. `Model` (capital M) is then the actual data you loop over.
