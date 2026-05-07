# Chat 1 - Prompt 36

## User
Implement what's missing / to do.

## Claude
Implemented all 6 missing items from Lab3.md:

**1. [Key] annotations** — Added to all 5 entities: RawLead, LeadIntelligence, JsonKeyStat, DesignTemplate, WeatherForecast.

**2. FK navigation properties + migration** — LeadIntelligence.LeadId now has [ForeignKey("Lead")] and a `virtual RawLead? Lead` nav property. RawLead has `virtual ICollection<LeadIntelligence> Intelligences`. Migration `AddLeadIntelligenceForeignKey` created and applied (adds FK constraint + index on LeadIntelligences.LeadId with CASCADE DELETE).

**3. Custom [Route] on 5 MVC actions** — RawLeadController now has [Route("leads")] at class level and custom routes on all actions:
- GET /leads → Index
- GET /leads/{id:int} → Detail
- GET /leads/create → Create
- GET /leads/{id:int}/edit → Edit
- POST /leads/{id:int}/delete → Delete

**4. semantic-model.md** — Created at repo root. Documents all 5 tables, their columns/types, and the 1-N RawLead→LeadIntelligence relationship.

**5. sitemap.md** — Created at repo root. Maps every URL in the app to controller/action/view, split into convention-routed, attribute-routed, and API sections.

**6. skills/ef-skill.md** — Created. Describes full EF workflow: adding entities, relationships, running migrations, soft-delete pattern, and project layout pointers.

Build: 0 errors, 1 pre-existing warning (unused SeedLabDataAsync).
