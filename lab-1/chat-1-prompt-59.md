# Chat 1 - Prompt 59

## User

For each feature add a link in the header, for some reason now it isn't showing neither home nor privacy, only when width goes below md breakpoint in hamburger menu

---

## Claude

Root cause: Tailwind's preflight CSS reset was overriding Bootstrap's `.collapse` class, keeping the desktop nav permanently hidden.

Fix: rewrote `_Layout.cshtml` navbar entirely in Tailwind, removed Bootstrap CSS dependency from layout.

- Desktop: `hidden md:flex` nav with links to Home, Weather, Raw Leads, Intelligence, Key Stats, Privacy
- Mobile: hamburger button toggles `#nav-mobile` via inline JS (no Bootstrap JS needed)
- Removed Bootstrap CSS link (Tailwind conflicts with it via preflight reset)
- Kept jQuery for form validation scripts
