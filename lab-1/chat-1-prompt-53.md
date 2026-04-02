# Chat 1 - Prompt 53

## User

Use option B so its separated.

---

## Claude

Created `Features/RawLead/RawLeadApiController.cs` inheriting from `ControllerBase` with `[ApiController]`.

Routes:
- GET    /api/rawleads        — list all active
- GET    /api/rawleads/{id}   — get single with JSON
- POST   /api/rawleads        — ingest from webhook/API
- PUT    /api/rawleads/{id}   — update
- DELETE /api/rawleads/{id}   — soft delete
