# Chat 1 - Prompt 47

## User

I need you to create the exact same thing like you did for Weather, but for "RawLead". It has to contain id, sourcesystem, externalId, RawJsonData and IngestedAt. This is a table schema for business leads i'll be getting. If you have any questions regarding design or my choices ask me before implementing it.

When I say i want you to do the same thing for RawLead as you did for Weather I literally mean everything, from entity to model, controller, view, ....

---

## Claude

Asked clarifying questions before implementing:

1. RawJsonData — `jsonb` (PostgreSQL native) or plain `string`?
2. IngestedAt — auto-set on insert or manually provided?
3. SourceSystem — free text or enum?
4. ExternalId — `string`?
5. Controller actions — form-based Create UI or API/webhook ingestion (no form)?
6. Immutability — edit allowed or append-only?
7. Index view — show metadata only + separate Detail view, or inline JSON?
