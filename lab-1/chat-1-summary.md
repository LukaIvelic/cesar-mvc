# Chat 1 Summary

## Topics Covered

### ASP.NET MVC Overview
- Corrected misconception that MVC replaces service/repository layers — the "M" is just a thin ViewModel, not the full data layer
- Spring MVC and ASP.NET MVC are nearly identical conceptually
- Three distinct model types: Entity (DB), ViewModel (output to view), Input model (from form)
- Built-in DI — register services in Program.cs via extension methods
- Controllers return `IActionResult` (View, RedirectToAction, Json, NotFound, etc.)
- Attribute routing mirrors Spring's `@GetMapping`

### Project Setup
- Using ASP.NET Core (.NET 10), NeonDB (serverless PostgreSQL), EF Core with Npgsql provider
- `dotnet-ef` must be installed as a global tool separately from the NuGet package
- EF Core migrations: `dotnet ef migrations add`, `dotnet ef database update`
- The `__EFMigrationsHistory` query failure on first run is expected — EF creates the table then proceeds

### Architecture Decisions
- **Feature folder structure** adopted over traditional flat layer folders (`Controllers/`, `Services/`, etc.)
- Interface + implementation combined in one file per feature concern
- DI registration extracted into `Extensions/ApplicationServiceExtensions.cs` and `Extensions/InfrastructureServiceExtensions.cs`
- `@page` in a Razor view turns it into a Razor Page (different system) — MVC views must NOT have `@page`

### Final Project Structure
```
cesar/
├── Data/                        AppDbContext
├── Extensions/                  DI registration
├── Features/
│   ├── Home/
│   │   ├── HomeController.cs
│   │   └── Models/
│   │       └── ErrorViewModel.cs
│   └── Weather/
│       ├── Entities/
│       │   └── WeatherForecast.cs
│       ├── Models/
│       │   ├── WeatherForecastViewModel.cs
│       │   └── CreateWeatherForecastModel.cs
│       ├── WeatherForecastController.cs
│       ├── WeatherRepository.cs
│       └── WeatherService.cs
├── Migrations/
├── Views/
├── lab-1/                       prompt logs
└── CLAUDE.md
```

### Key Routes (Weather)
- `GET /WeatherForecast` → Index (list all)
- `GET /WeatherForecast/Create` → Create form
- `POST /WeatherForecast/Create` → Submit form
