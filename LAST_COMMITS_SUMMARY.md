# Lab 5 Code Change Summary

Generated: 2026-06-12  
Commit: `8fe8560` - `Implement lab 5 API auth upload tests`  
Date: 2026-06-11  
Scale: 55 files changed, 3002 insertions, 129 deletions

## Summary

Lab 5 turns the app into a more complete authenticated MVC/API system. The commit adds ASP.NET Core Identity, role-based authorization, REST API DTOs/controllers, design-template file attachments, database migration updates, and integration tests for API CRUD and authorization behavior.

## Authentication and Authorization

- Added ASP.NET Core Identity using custom `AppUser`.
- Added role constants through `AppRoles`.
- Added role seeding through `IdentitySeed`.
- Added account flows for:
  - register;
  - login;
  - logout;
  - external login confirmation;
  - access denied.
- Added optional Google external login configuration through `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret`.
- Updated `Program.cs` to register Identity, authentication, authorization, and role seeding.
- Protected MVC and API write operations with role checks:
  - reads are generally anonymous;
  - create/update require `Admin` or `Manager`;
  - delete requires `Admin`.

Key files:

- `Features/Identity/AccountController.cs`
- `Features/Identity/AppRoles.cs`
- `Features/Identity/Entities/AppUser.cs`
- `Features/Identity/IdentitySeed.cs`
- `Views/Account/*.cshtml`
- `Views/Shared/_LoginPartial.cshtml`
- `Program.cs`

## API Work

Lab 5 adds or expands JSON API coverage across the main app resources.

Added API controller:

- `Features/DesignTemplates/DesignTemplateApiController.cs`
- `Features/Weather/WeatherForecastApiController.cs`

Updated API controllers:

- `Features/RawLead/RawLeadApiController.cs`
- `Features/JsonKeyStats/JsonKeyStatApiController.cs`
- `Features/LeadIntelligence/LeadIntelligenceApiController.cs`

API behavior added or standardized:

- list/search endpoints with optional `q` query parameter;
- detail endpoints returning `404` for missing or soft-deleted records;
- create endpoints returning `201 Created`;
- update endpoints validating route/body ID consistency;
- delete endpoints returning `204 No Content`;
- validation responses returning `400 Bad Request`;
- DTO-based response shapes instead of exposing full entities directly.

DTO/model files added:

- `Features/DesignTemplates/Models/CreateDesignTemplateDto.cs`
- `Features/DesignTemplates/Models/DesignTemplateDto.cs`
- `Features/DesignTemplates/Models/DesignTemplateAttachmentDto.cs`
- `Features/DesignTemplates/Models/UpdateDesignTemplateDto.cs`
- `Features/JsonKeyStats/Models/JsonKeyStatDto.cs`
- `Features/LeadIntelligence/Models/LeadIntelligenceDto.cs`
- `Features/RawLead/Models/RawLeadDto.cs`
- `Features/Weather/Models/CreateWeatherForecastDto.cs`
- `Features/Weather/Models/UpdateWeatherForecastDto.cs`
- `Features/Weather/Models/WeatherForecastDto.cs`

## Design Template Attachments

Lab 5 adds upload support for files attached to design templates.

Main behavior:

- Added `DesignTemplateAttachment` entity.
- Added `DesignTemplate.Attachments` navigation.
- Added attachment metadata to API DTO responses.
- Added MVC endpoints to upload, list, and delete attachments.
- Stores uploaded files under `wwwroot/uploads/design-templates/{templateId}`.
- Persists attachment metadata including original file name, stored path, content type, file size, and creation time.
- Deletes the physical file when an attachment is removed.
- Adds `_Attachments.cshtml` partial for rendering attachment lists in the edit screen.

Key files:

- `Features/DesignTemplates/Entities/DesignTemplateAttachment.cs`
- `Features/DesignTemplates/DesignTemplateController.cs`
- `Features/DesignTemplates/DesignTemplateRepository.cs`
- `Features/DesignTemplates/Models/DesignTemplateAttachmentDto.cs`
- `Views/DesignTemplate/Edit.cshtml`
- `Views/DesignTemplate/_Attachments.cshtml`

## Database and Configuration

- Changed `AppDbContext` to inherit from `IdentityDbContext<AppUser>`.
- Added `DbSet<DesignTemplateAttachment>`.
- Added migration `AddIdentityAndDesignTemplateAttachments`.
- Updated the EF model snapshot with Identity tables and attachment relationships.
- Updated app settings with authentication configuration placeholders.
- Added Identity-related package/project references.
- Added test project to the solution.

Key files:

- `Data/AppDbContext.cs`
- `Migrations/20260611152138_AddIdentityAndDesignTemplateAttachments.cs`
- `Migrations/AppDbContextModelSnapshot.cs`
- `appsettings.json`
- `cesar.csproj`
- `cesar.slnx`

## Tests

Lab 5 adds an integration test project that runs the app through `WebApplicationFactory`.

Covered scenarios:

- Raw lead API CRUD, search, missing records, and invalid JSON.
- JSON key stat API CRUD, search, missing records, and invalid values.
- Lead intelligence API CRUD, search, missing records, and invalid values.
- Design template API CRUD, search, missing records, and invalid placeholder schema JSON.
- Weather forecast API CRUD, search, missing records, and invalid temperature.
- Anonymous users are rejected from protected API write endpoints.

Test infrastructure:

- Uses EF Core in-memory database per test factory.
- Replaces the app authentication scheme with a test auth handler.
- Can create authenticated or anonymous clients.
- Authenticated test users receive both `Admin` and `Manager` roles.

Key files:

- `cesar.Tests/ApiCrudTests.cs`
- `cesar.Tests/CesarWebApplicationFactory.cs`
- `cesar.Tests/cesar.Tests.csproj`

## Net Effect

Lab 5 adds the project’s first complete auth/API/test layer. After this commit, the app has role-gated write operations, public read APIs, DTO responses, uploadable design-template attachments, Identity-backed users/roles, and automated integration coverage for the main API resources.
