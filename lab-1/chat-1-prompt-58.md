# Chat 1 - Prompt 58

## User

Error inserting multiple rawleads at once — 400 validation error, JSON array not accepted by single-object endpoint.

---

## Claude

Added bulk insert support:
- `IRawLeadRepository.AddRangeAsync()` — single SaveChanges for all entities
- `IRawLeadService.CreateBulkAsync()` — builds entities, saves range, deduplicates keys across batch, tracks all
- `POST /api/rawleads/bulk` — accepts `IEnumerable<CreateRawLeadModel>`, validates JSON on each entry, returns 400 with offending externalIds if any are invalid

Single insert endpoint unchanged. Bulk returns count of ingested leads.
