# Chat 1 - Prompt 54

## User

Create LeadIntelligence feature (LeadId, ContentHash, FamiliarityIndex 0-1, DataDensityScore, LastAnalyzedAt) and a KeyOccurrence-style table (Key, Occurrences). Both with full feature structure identical to Weather/RawLead.

---

## Claude

Asked design questions before implementing:
1. LeadId — foreign key constraint or loose reference?
2. ContentHash — unique constraint? Algorithm?
3. DataDensityScore — 0-1 range or different?
4. Soft delete on both tables?
5. Name for key occurrence table?
6. Key column — globally unique or duplicates allowed?
