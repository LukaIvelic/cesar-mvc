# Chat 1 - Prompt 20

## User

Make files that are related go into a related directory if that's a convention. If we continue this way, we're creating a God folder. Put together everything that can be.

---

## Claude

Reorganized to feature folder structure. Everything weather-domain-related moved to `Features/Weather/` with namespace `cesar.Features.Weather`.

**Deleted:**
- `Entities/` folder
- `Repositories/` folder
- `Services/` folder
- `Models/WeatherForecastViewModel.cs`
- `Models/CreateWeatherForecastModel.cs`

**Created:**
- `Features/Weather/WeatherForecast.cs`
- `Features/Weather/IWeatherRepository.cs`
- `Features/Weather/WeatherRepository.cs`
- `Features/Weather/IWeatherService.cs`
- `Features/Weather/WeatherService.cs`
- `Features/Weather/WeatherForecastViewModel.cs`
- `Features/Weather/CreateWeatherForecastModel.cs`

**Updated namespaces in:**
- `Data/AppDbContext.cs`
- `Controllers/WeatherForecastController.cs`
- `Extensions/ApplicationServiceExtensions.cs`
- `Extensions/InfrastructureServiceExtensions.cs`
- `Views/WeatherForecast/Index.cshtml`
- `Views/WeatherForecast/Create.cshtml`
- `Migrations/AppDbContextModelSnapshot.cs` (entity string reference)

**Final structure:**
```
cesar/
├── Controllers/       (MVC convention - stays at root)
├── Views/             (MVC convention - stays at root)
├── Models/            (only ErrorViewModel remains)
├── Data/              (AppDbContext - infrastructure)
├── Extensions/        (DI registration)
├── Features/
│   └── Weather/       (everything weather-related)
└── Migrations/        (auto-generated)
```
