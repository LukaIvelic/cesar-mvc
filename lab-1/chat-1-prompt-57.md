# Chat 1 - Prompt 57

## User

How would rawlead depend on jsonkeystats? When you get rawjsondata, you extract keys if there are keys map them and call jsonkeystats post call or create method

---

## Claude

Updated RawLeadService to inject IJsonKeyStatService and call TrackKeysAsync after every CreateAsync.

Flow:
1. Save RawLead to DB
2. Parse RawJsonData with JsonDocument
3. Extract top-level keys (only if root is an object)
4. Call _keyStatService.TrackKeysAsync(keys) — upserts each key

ExtractKeys() is a private static helper that safely parses the JSON and returns an empty list on failure.
