## User

add breadcrumbs on all pages, use layout.cshtml if possible so that breadcrumbs all have the same logic and design. If its not possible add to every page individually.

## Claude

Implemented breadcrumbs in the layout:

**Created:**
- `Views/Shared/_Breadcrumbs.cshtml` partial view with centralized breadcrumb rendering logic
- `Extensions/BreadcrumbExtensions.cs` with SetBreadcrumbs() and SetCurrentPage() helper methods

**Updated:**
- `Views/Shared/_Layout.cshtml` — includes breadcrumb partial before RenderBody()
- All 6 controllers — added breadcrumb data via ViewData in each action

**Design:**
- Breadcrumbs: Home › Section › Page
- Separator: › (alt+155)
- Current page bolded, earlier items are links
- Home page has no breadcrumbs
- Detail/Edit pages show IDs: "Home › Raw Leads › #123"

**Controllers updated:** RawLead, DesignTemplate, LeadIntelligence, JsonKeyStat, WeatherForecast, Home

Commit: feat: add breadcrumb navigation on all pages
