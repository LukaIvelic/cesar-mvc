# Chat 1 - Prompt 56

## User

Do we handle automatically updating json key stats when we insert rawdata?

---

## Claude

No — RawLeadService.CreateAsync() just saves the lead, completely disconnected from JsonKeyStats.

Fix: inject IJsonKeyStatService into RawLeadService, extract JSON keys after saving, call TrackKeysAsync(). Creates cross-feature dependency (RawLead → JsonKeyStats) — acceptable at this project size. Awaiting confirmation to implement.
