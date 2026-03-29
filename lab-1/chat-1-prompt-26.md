# Chat 1 - Prompt 26

## User

[Screenshot: Firefox can't connect to localhost:7115/WeatherForecast/Create]

Here's the image and the route: https://localhost:7115/WeatherForecast/Create. I can't access it

---

## Claude

Two issues found from deleting root `Models/`:

1. `Views/_ViewImports.cshtml` had `@using cesar.Models` — updated to use `cesar.Features.Home.Models` and `cesar.Features.Weather.Models`
2. `Views/Shared/Error.cshtml` had `@model ErrorViewModel` — updated to fully qualified `cesar.Features.Home.Models.ErrorViewModel`

Build confirmed clean (only error was file lock because old process was still running).

Fix: stop the running process (Ctrl+C) and run `dotnet run` again.
