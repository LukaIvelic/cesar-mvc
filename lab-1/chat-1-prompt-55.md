# Chat 1 - Prompt 55

## User

Answers:
1. LeadId loose reference
2. SHA256, duplicates allowed (different content types)
3. DataDensityScore 0-1
4. Always soft delete
5. Named JsonKeyStats
6. Globally unique key (one row per distinct key across all leads)

---

## Claude

Created two full features:

**LeadIntelligence** (`Features/LeadIntelligence/`)
- Entity: Id, LeadId, ContentHash, FamiliarityIndex (double 0-1), DataDensityScore (double 0-1), LastAnalyzedAt, ValidFrom, ValidTo
- Full CRUD: MVC controller + API controller
- Extra API: GET /api/leadintelligence/hash/{hash}, POST /api/leadintelligence/compute-hash
- SHA256 computed via `ComputeSha256()` on service, used `global::` alias to resolve namespace conflict

**JsonKeyStats** (`Features/JsonKeyStats/`)
- Entity: `JsonKeyStat` (singular, no namespace conflict), Id, Key, Occurrences, ValidFrom, ValidTo
- Full CRUD: MVC controller + API controller
- Extra: `TrackKeysAsync(keys)` bulk upsert, `IncrementAsync(key)` single increment
- API: POST /api/jsonkeystats/track, POST /api/jsonkeystats/increment/{key}
- Results ordered by Occurrences descending

Views: Index.cshtml, Create.cshtml, Edit.cshtml for both (no Detail needed - no large data fields)

Run:
```bash
dotnet ef migrations add AddLeadIntelligenceAndJsonKeyStats
dotnet ef database update
```
