## User

This markdown file contains what I need to have in this project. Tell me what do I have and what am I missing?

(Attached: Untitled document(1).md — Lab 2 HTML/Binding requirements, due 2026-04-17)

## Claude

Gap analysis against Lab 2 requirements:

**What you have:**
- Feature folder structure with entities, services, repositories, controllers for all 5 entities (RawLead, DesignTemplate, LeadIntelligence, JsonKeyStat, WeatherForecast)
- Index/list views for all entities
- Detail view for RawLead only
- Navigation menu in _Layout.cshtml (Tailwind, not default Bootstrap)
- lab-1/ prompt logs

**What's missing:**

| Requirement | Status |
|---|---|
| UX sub-agent instruction file committed to Git | Missing — no .claude/agents/ux-agent.md or equivalent |
| Log proving sub-agent was spawned for UI generation | Missing |
| Mock repositories with static data (Lab 1 data) | Missing — all repos use real EF Core / DB |
| Details view for DesignTemplate | Missing |
| Details view for JsonKeyStat | Missing |
| Details view for LeadIntelligence | Missing |
| Details view for WeatherForecast | Missing |
| Custom home page (not the ASP.NET default) | Missing — current home is just "Welcome" with a MS docs link |
| Breadcrumbs navigation | Missing |
| Links from list rows → details | Need to verify on each Index view |
| Unique/non-standard UX | Partial — has Tailwind but needs to be visually distinct/custom |

**Priority order to fix:**
1. Create the UX sub-agent instruction file (agents/ux-agent.md) and commit it
2. Switch to mock repositories (static in-memory data, no DB)
3. Add Details views + controller actions for the 4 missing entities
4. Build a custom home page (dashboard-style)
5. Add breadcrumbs to layouts/views
6. Verify list→details links on Index pages
7. Make UX visually unique (non-standard design)
