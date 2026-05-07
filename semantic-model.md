# Semantic DB Model

## Tables / Entities

### RawLead
Table: `RawLeads`

| Property | Type | Notes |
|---|---|---|
| Id | int | PK |
| SourceSystem | string | e.g. `web_form`, `crm_sync` |
| ExternalId | string | ID from source system |
| RawJsonData | string (jsonb) | raw payload from ingest |
| IngestedAt | DateTime | when the record arrived |
| ValidFrom | DateTime | start of validity window |
| ValidTo | DateTime? | null = currently active |

### LeadIntelligence
Table: `LeadIntelligences`

| Property | Type | Notes |
|---|---|---|
| Id | int | PK |
| LeadId | int | FK → RawLeads.Id |
| Lead | RawLead | navigation property |
| ContentHash | string | hash of analysed content |
| FamiliarityIndex | double | 0–100 score |
| DataDensityScore | double | 0–100 score |
| LastAnalyzedAt | DateTime | last analysis timestamp |
| ValidFrom | DateTime | start of validity window |
| ValidTo | DateTime? | null = currently active |

### JsonKeyStat
Table: `JsonKeyStats`

| Property | Type | Notes |
|---|---|---|
| Id | int | PK |
| Key | string | JSON field name |
| Occurrences | int | how many leads contain this key |
| ValidFrom | DateTime | start of validity window |
| ValidTo | DateTime? | null = currently active |

### DesignTemplate
Table: `DesignTemplates`

| Property | Type | Notes |
|---|---|---|
| Id | int | PK |
| Name | string | human-readable template name |
| HtmlMarkup | string (text) | Mustache-style HTML with `{{placeholder}}` |
| PlaceholderSchema | string (jsonb) | default values for all placeholders |
| ContentType | ContentType (enum) | `Mail`, `HTML`, `SMS` |
| ValidFrom | DateTime | start of validity window |
| ValidTo | DateTime? | null = currently active |

### WeatherForecast
Table: `WeatherForecasts`

| Property | Type | Notes |
|---|---|---|
| Id | int | PK |
| Date | DateOnly | forecast date |
| TemperatureC | int | Celsius |
| Summary | string? | optional description |
| TemperatureF | int | computed, not stored |

## Relationships

```
RawLead  1 ──────────── N  LeadIntelligence
         (RawLeads.Id)       (LeadIntelligences.LeadId FK, CASCADE DELETE)
```

All other entities are standalone (no FK relationships).

## Soft-delete pattern

`RawLead`, `LeadIntelligence`, `JsonKeyStat`, and `DesignTemplate` use a soft-delete pattern: `ValidTo = null` means active; setting `ValidTo = DateTime.UtcNow` marks the record as deleted without removing it from the database.
