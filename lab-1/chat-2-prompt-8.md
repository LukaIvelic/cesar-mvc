## User

so what exactly does this UX subagent do? Does he check the UX quality or what?

## Claude

Explained that the UX sub-agent is currently a documentation artifact, not an active quality checker.

**What it IS:**
- An instruction file (.claude/agents/ux-agent.md) defining the design system
- A reference the main agent reads when generating UI code
- Evidence for the grader that a sub-agent was defined and invoked

**What it does NOT do:**
- Does not automatically check views for UX quality
- Does not enforce anything at build or runtime

**How it's supposed to work:**
When prompted to generate UI, the main agent spawns the ux-agent, which reads the instruction file and generates/reviews UI with those constraints. The spawn gets logged.

**For Lab 2:** The grader checks that the file exists and is committed, there's a spawn log, and the resulting UI is non-standard. The sub-agent is less "quality checker" and more "design system prompt that travels with the repo."
