# Sitemap — URL Routing Model

## Convention-routed controllers (default pattern `{controller}/{action}/{id?}`)

### HomeController (`Features/Home/HomeController.cs`)

| URL | HTTP | Controller | Action | View |
|---|---|---|---|---|
| `/` | GET | Home | Index | `Views/Home/Index.cshtml` |
| `/Home/Privacy` | GET | Home | Privacy | `Views/Home/Privacy.cshtml` |
| `/Home/Error` | GET | Home | Error | `Views/Shared/Error.cshtml` |

### LeadIntelligenceController (`Features/LeadIntelligence/LeadIntelligenceController.cs`)

| URL | HTTP | Controller | Action | View |
|---|---|---|---|---|
| `/LeadIntelligence` | GET | LeadIntelligence | Index | `Views/LeadIntelligence/Index.cshtml` |
| `/LeadIntelligence/Create` | GET | LeadIntelligence | Create | `Views/LeadIntelligence/Create.cshtml` |
| `/LeadIntelligence/Create` | POST | LeadIntelligence | Create | redirect → Index |
| `/LeadIntelligence/Edit/{id}` | GET | LeadIntelligence | Edit | `Views/LeadIntelligence/Edit.cshtml` |
| `/LeadIntelligence/Edit/{id}` | POST | LeadIntelligence | Edit | redirect → Index |
| `/LeadIntelligence/Delete/{id}` | POST | LeadIntelligence | Delete | redirect → Index |

### JsonKeyStatController (`Features/JsonKeyStats/JsonKeyStatController.cs`)

| URL | HTTP | Controller | Action | View |
|---|---|---|---|---|
| `/JsonKeyStat` | GET | JsonKeyStat | Index | `Views/JsonKeyStat/Index.cshtml` |
| `/JsonKeyStat/Create` | GET | JsonKeyStat | Create | `Views/JsonKeyStat/Create.cshtml` |
| `/JsonKeyStat/Create` | POST | JsonKeyStat | Create | redirect → Index |
| `/JsonKeyStat/Edit/{id}` | GET | JsonKeyStat | Edit | `Views/JsonKeyStat/Edit.cshtml` |
| `/JsonKeyStat/Edit/{id}` | POST | JsonKeyStat | Edit | redirect → Index |
| `/JsonKeyStat/Delete/{id}` | POST | JsonKeyStat | Delete | redirect → Index |

### DesignTemplateController (`Features/DesignTemplates/DesignTemplateController.cs`)

| URL | HTTP | Controller | Action | View |
|---|---|---|---|---|
| `/DesignTemplate` | GET | DesignTemplate | Index | `Views/DesignTemplate/Index.cshtml` |
| `/DesignTemplate/Create` | GET | DesignTemplate | Create | `Views/DesignTemplate/Create.cshtml` |
| `/DesignTemplate/Create` | POST | DesignTemplate | Create | redirect → Index |
| `/DesignTemplate/Edit/{id}` | GET | DesignTemplate | Edit | `Views/DesignTemplate/Edit.cshtml` |
| `/DesignTemplate/Edit/{id}` | POST | DesignTemplate | Edit | redirect → Index |
| `/DesignTemplate/Delete/{id}` | POST | DesignTemplate | Delete | redirect → Index |
| `/DesignTemplate/Preview/{id}` | GET | DesignTemplate | Preview | `Views/DesignTemplate/Preview.cshtml` |
| `/DesignTemplate/Preview/{id}` | POST | DesignTemplate | Preview | `Views/DesignTemplate/Preview.cshtml` |
| `/DesignTemplate/PreviewDraft` | POST | DesignTemplate | PreviewDraft | inline HTML (Content result) |

### WeatherForecastController (`Features/Weather/WeatherForecastController.cs`)

| URL | HTTP | Controller | Action | View |
|---|---|---|---|---|
| `/WeatherForecast` | GET | WeatherForecast | Index | `Views/WeatherForecast/Index.cshtml` |
| `/WeatherForecast/Create` | GET | WeatherForecast | Create | `Views/WeatherForecast/Create.cshtml` |

---

## Attribute-routed controller — RawLeadController (`Features/RawLead/RawLeadController.cs`)

Custom routes defined via `[Route]` attributes (controller prefix: `/leads`).

| URL | HTTP | Controller | Action | View |
|---|---|---|---|---|
| `/leads` | GET | RawLead | Index | `Views/RawLead/Index.cshtml` |
| `/leads/{id:int}` | GET | RawLead | Detail | `Views/RawLead/Detail.cshtml` |
| `/leads/create` | GET | RawLead | Create | `Views/RawLead/Create.cshtml` |
| `/leads/create` | POST | RawLead | Create | redirect → `/leads` |
| `/leads/{id:int}/edit` | GET | RawLead | Edit | `Views/RawLead/Edit.cshtml` |
| `/leads/{id:int}/edit` | POST | RawLead | Edit | redirect → `/leads` |
| `/leads/{id:int}/delete` | POST | RawLead | Delete | redirect → `/leads` |

---

## API controllers (attribute-routed, JSON responses)

| URL prefix | Controller | File |
|---|---|---|
| `/api/rawleads` | RawLeadApiController | `Features/RawLead/RawLeadApiController.cs` |
| `/api/leadintelligence` | LeadIntelligenceApiController | `Features/LeadIntelligence/LeadIntelligenceApiController.cs` |
| `/api/jsonkeystats` | JsonKeyStatApiController | `Features/JsonKeyStats/JsonKeyStatApiController.cs` |

---

## Shared views (partials / layout)

| File | Purpose |
|---|---|
| `Views/Shared/_Layout.cshtml` | Main site layout with nav |
| `Views/Shared/_Breadcrumbs.cshtml` | Breadcrumb partial |
| `Views/Shared/Error.cshtml` | Error page |
| `Views/Shared/_ValidationScriptsPartial.cshtml` | Client-side validation scripts |
| `Views/_ViewStart.cshtml` | Sets `_Layout` as default layout |
| `Views/_ViewImports.cshtml` | Global using directives and tag helpers |
