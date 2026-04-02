# Chat 1 - Prompt 45

## User

Tell me what is injected and what isn't in weatherforecast. I see IWeatherRepository interface being passed in the constructor, but what I don't see is where is it passed

---

## Claude

The DI chain across three files:

1. `InfrastructureServiceExtensions.cs` — registers `IWeatherRepository → WeatherRepository`
2. `ApplicationServiceExtensions.cs` — registers `IWeatherService → WeatherService`
3. `Program.cs` — calls both extension methods at startup

When a request hits `/WeatherForecast`, ASP.NET resolves the full tree automatically:
- Controller needs `IWeatherService` → creates `WeatherService`
- `WeatherService` needs `IWeatherRepository` → creates `WeatherRepository`
- `WeatherRepository` needs `AppDbContext` → creates that too

No `new` calls anywhere. Container builds the whole graph. Spring equivalent: `@Autowired` + component scanning, except here registration is explicit in `Program.cs`.
