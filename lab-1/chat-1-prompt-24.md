# Chat 1 - Prompt 24

## User

Why are controllers in their separate controllers directory?

---

## Claude

They don't need to be. That's just the default MVC template convention. ASP.NET Core discovers controllers by scanning for classes that inherit from `Controller` — it doesn't care what folder they're in. In feature-folder architecture the controller belongs inside the feature.

Moved:
- `Controllers/HomeController.cs` → `Features/Home/HomeController.cs`
- `Controllers/WeatherForecastController.cs` → `Features/Weather/WeatherForecastController.cs`

Deleted `Controllers/` folder. Updated `CLAUDE.md`.

Final structure:
```
Features/
├── Home/
│   ├── HomeController.cs
│   └── Models/
│       └── ErrorViewModel.cs
└── Weather/
    ├── Entities/
    │   └── WeatherForecast.cs
    ├── Models/
    │   ├── WeatherForecastViewModel.cs
    │   └── CreateWeatherForecastModel.cs
    ├── WeatherForecastController.cs
    ├── WeatherRepository.cs
    └── WeatherService.cs
```

Views still resolve correctly — ASP.NET looks up views by controller class name, not file location.
