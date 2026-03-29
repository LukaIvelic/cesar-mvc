# Chat 1 - Prompt 18

## User

Why didn't we addd anything to models? Have they become useless now or?

---

## Claude

Not useless — we skipped a step for simplicity.

Right now the controller passes the **entity** directly to the view:
```csharp
return View(forecasts); // forecasts = IEnumerable<WeatherForecast> (entity)
```

In a real app you'd map to a **ViewModel** first and pass that instead:
```csharp
var viewModels = forecasts.Select(f => new WeatherForecastViewModel
{
    Date = f.Date,
    TemperatureC = f.TemperatureC,
    Summary = f.Summary
});
return View(viewModels);
```

The `Models/` folder is where `WeatherForecastViewModel` would live.

**Why bother?** Three reasons:

1. **Don't leak DB internals to the UI** — your entity might have fields the view shouldn't see (internal IDs, foreign keys, sensitive data)
2. **Shape data for the view** — ViewModel can combine fields from multiple entities, add computed display strings, etc.
3. **Input models** — when a user submits a form, you receive a model from `Models/` (e.g. `CreateWeatherForecastModel`), validate it, then map it to an entity before saving

So the full correct flow is:

```
DB → Entity → Repository → Service → Controller → ViewModel → View
                                              ↑
                                         Models/ folder
```

We short-circuited it by passing the entity straight through. Fine for learning, but not what you'd do in production.
