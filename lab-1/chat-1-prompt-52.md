# Chat 1 - Prompt 52

## User

Okay do we have an API which you can call to input data?

---

## Claude

No — only HTML form POST exists. Need a JSON API endpoint.

Two options:
A. Add `[HttpPost("api/rawleads")]` with `[FromBody]` to existing controller
B. Separate `RawLeadApiController : ControllerBase` with `[ApiController]` — cleaner separation

Option B preferred: `Controller` is for MVC views, `ControllerBase` is for APIs. Awaiting confirmation to implement.
