# Chat 1 - Prompt 29

## User

Okay, instead of displaying weather forecast data like you are now. Display it using a table. Also give it an action column at the end of the row.

Answer me: is there any component library I could use to make the development of frontend easier in ASP.NET*

---

## Claude

Updated `Views/WeatherForecast/Index.cshtml` to use a `<table>` with Date, Temperature, Summary, and Actions columns. Added Edit and Delete links in the action column using `asp-action` and `asp-route-id` tag helpers.

Added `Id` to `WeatherForecastViewModel` (needed for routing action links) and updated the controller mapping to populate it.

### Component Libraries Answer

No React-like component library exists for Razor MVC. Options:

- **Bootstrap** — already in the project, handles layout/buttons/tables
- **DataTables.js** — one-liner to add sort/filter/pagination to any `<table>`
- **AG Grid** — best-in-class data grid, free tier
- **Tailwind CSS** — needs Node build step
- **Telerik / Syncfusion / DevExpress** — paid ASP.NET-specific suites with Tag Helper components

Recommendation: Bootstrap (already there) + DataTables.js for tables.
