# Project Criteria Compliance

This file maps the project implementation to `Kriteriji-projekt-pragovi.pdf`.

| Criterion | Implementation |
| --- | --- |
| Deploy on cloud provider or VM | `Dockerfile`, `.dockerignore`, `.github/workflows/azure-webapp.yml`, and `.github/workflows/google-cloud-run.yml` support Azure Web App and Google Cloud Run deployment. |
| Tests for all API endpoints | `cesar.Tests/ApiCrudTests.cs` covers CRUD endpoints plus secondary API routes such as autocomplete, bulk ingest, key tracking, hashing, AI analysis, global search, and MCP. |
| AI integration | Lead intelligence analysis and AI template generation use OpenAI configuration through `OpenAI:ApiKey`. |
| Google Sign-In | External Google login is implemented in `AccountController` and visible on `/Account/Login`; configure `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret` to enable the live provider. |
| Global search | `/search` and `/api/search` search pages, menu destinations, raw leads, templates, JSON keys, and intelligence records. |
| Logging mechanism | `CesarFileLoggerProvider` writes application logs to a file path from `Logging:File:Path`, or `logs/cesar.log` by default. |
| Responsive mobile/web UI | Shared layout uses a responsive sidebar/topbar shell and mobile navigation toggle. |
| CRUD works without errors | API CRUD tests cover create, read, update, delete, validation, missing records, and unauthorized write access. |
| Expose MCP and agentic IDE access | `/mcp` exposes a minimal streamable HTTP MCP-style JSON-RPC endpoint, and `mcp.json` points agentic IDEs to the local server. |
| Overall functionality | Build and API tests are used as the project health check. |
