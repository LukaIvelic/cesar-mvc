# Chat 1 - Prompt 42

## User

wwwroot/lib/bootstrap and jquery are good to go on git?

---

## Claude

No `libman.json` found — files were dumped by the template with no restore mechanism. Three options:

1. **Keep in git** — simple, works, ~2MB of vendor files in history
2. **LibMan** — add `libman.json`, gitignore `wwwroot/lib/`, restore with `dotnet tool run libman restore`
3. **CDN links** — replace local files with CDN tags in `_Layout.cshtml`, nothing to manage

Since Tailwind is already handling CSS, Bootstrap styles are redundant. Recommended: Option 3 — CDN for only what's actually needed (likely just jQuery for validation).

Awaiting user decision.
