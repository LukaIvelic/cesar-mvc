# Chat 1 - Prompt 36

## User

Okay it works now, tell me the root cause

---

## Claude

Two separate root causes:

1. **Tailwind v4 breaking changes** — v4 removed the standalone CLI binary and `init` command. We were following v3 instructions. Build in v4 goes through PostCSS only.

2. **Windows + Git Bash binary resolution** — npm scripts run via `cmd.exe` on Windows, not Git Bash. `node_modules/.bin` on Windows has `.cmd` wrappers, not Unix executables. Resolution fails in certain environments.

Fix `node ./node_modules/postcss-cli/index.js` bypasses both — telling Node directly to run a JS file, no binary resolution needed.
