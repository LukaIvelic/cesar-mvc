# Chat 1 - Prompt 25

## User

How to access createWeatherForecast route?

Before running out of context, write a summary of our conversation where you write chats in format "chat-1-summary". Add that to claude.md so you know to automatically do it once you almost run out of context

---

## Claude

The default route pattern is `{controller}/{action}` where controller = class name minus "Controller":

- `GET /WeatherForecast/Create` — renders the form
- `POST /WeatherForecast/Create` — submits the form

The `[HttpGet]` and `[HttpPost]` attributes on the two `Create` actions are what distinguish them — same URL, different HTTP method.

Created `lab-1/chat-1-summary.md` with full conversation summary. Updated `CLAUDE.md` with the rule to write a summary before context runs out.
