# Chat 1 - Prompt 19

## User

Use it as if it were in production. It's useless to skip steps. I'm not learning anything if you do it that way.

Explain more on this 3 points you made and give me concrete examples:

Don't leak DB internals to the UI — your entity might have fields the view shouldn't see (internal IDs, foreign keys, sensitive data)
Shape data for the view — ViewModel can combine fields from multiple entities, add computed display strings, etc.
Input models — when a user submits a form, you receive a model from Models/ (e.g. CreateWeatherForecastModel), validate it, then map it to an entity before saving

---

## Claude

### Point 1 — Don't leak DB internals

Your entity has `Id` — a database primary key. The view has no business knowing it. In a real app entities often also have `CreatedAt`, `UpdatedAt`, foreign key IDs like `UserId`, or fields like `PasswordHash`. If you pass the entity directly, all of that is accessible in the template.

**Rule:** the view gets only what it needs to render, nothing else.

### Point 2 — Shape data for the view

`TemperatureC` and `TemperatureF` are two separate fields. Instead of letting the view combine them, the ViewModel does it once and exposes a `TemperatureDisplay` string like `"23°C / 73°F"`. The view becomes dumb — it just renders strings, no logic.

**Rule:** any display formatting happens in the ViewModel, not in the template.

### Point 3 — Input models

When a user submits a form:
1. Bind raw form data to a typed input model
2. Validate with data annotations (`[Required]`, `[Range]`)
3. If valid — map to entity and save
4. If invalid — return the form with error messages

**Rule:** everything that crosses the HTTP boundary goes through a model in `Models/`.

### What was added/changed:

- `Models/WeatherForecastViewModel.cs` — output model (Date, TemperatureDisplay, Summary). No Id, no raw numbers.
- `Models/CreateWeatherForecastModel.cs` — input model with [Required], [Range], [MaxLength] validation.
- `Controllers/WeatherForecastController.cs` — Index maps entity → ViewModel. Create GET returns empty form. Create POST validates input model, maps to entity, saves.
- `Views/WeatherForecast/Index.cshtml` — now uses WeatherForecastViewModel, renders TemperatureDisplay directly.
- `Views/WeatherForecast/Create.cshtml` — form bound to CreateWeatherForecastModel with validation spans.
