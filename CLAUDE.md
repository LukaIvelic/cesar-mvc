# Claude Rules

## Prompt Logging — DO THIS EVERY SINGLE RESPONSE, NO EXCEPTIONS

After **every** response, immediately save to `lab-1/` before doing anything else in the next turn.

- File naming: `chat-{N}-prompt-{M}.md` where N = chat number, M = prompt number within that chat
- Prompt number increments with each new prompt in the same chat
- Chat number increments when a new chat begins — check existing files in `lab-1/` to determine the correct next N
- Each file contains both the user's prompt and Claude's full response under `## User` and `## Claude` headers
- **NEVER skip this. The user is paying for this service and has had to ask multiple times.**

## Feature Folder Structure — USE THIS FOR EVERY NEW FEATURE

All domain logic lives in `Features/{FeatureName}/`. Never create flat top-level folders like `Services/`, `Repositories/`, or `Models/`.

```
Features/{FeatureName}/
├── Entities/                   ← EF Core DB-mapped classes
├── Models/                     ← ViewModels (output) and input/form models
├── {Feature}Controller.cs      ← controller lives inside the feature
├── {Feature}Repository.cs      ← interface + implementation in one file
└── {Feature}Service.cs         ← interface + implementation in one file
```

**Rules:**
- Controller belongs inside the feature folder — ASP.NET discovers controllers by class, not folder
- Interface and implementation go in the same file (they always travel together)
- `Views/` stays at root — MVC view resolution is based on controller class name, not file location
- `Data/` stays at root — AppDbContext is infrastructure, not a feature
- `Extensions/` stays at root — app-wide DI registration
- `Migrations/` stays at root — EF Core auto-generated

## Conversation Summaries — WRITE ONE BEFORE CONTEXT RUNS OUT

When the conversation is getting long or context is approaching its limit, write a summary file to `lab-1/chat-{N}-summary.md` before the context is lost.

Format:
- File name: `chat-{N}-summary.md` matching the current chat number
- Sections: Topics Covered, Architecture Decisions, Final Project Structure, Key Routes
- Keep it dense and factual — this is a reference for future chats, not a narrative
- Add a pointer in this CLAUDE.md if the project state changed significantly
