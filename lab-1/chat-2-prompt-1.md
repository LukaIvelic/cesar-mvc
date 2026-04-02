# Lab 1 Prompt Log

## User Prompt
Okay create a local seeding method, but don't use it because I already have data. You can use the dotnet bogus library for generating random data if you want. Use Linq queries to sort them desc and also .where .filter .map and .ToListAsync somewhere so that we can say we have everything we need. Also write my prompt and your response into /lab-1 directory, see how markup files are used there and save this conversation.

# Lab 1 Response Log

A local `SeedLabDataAsync` method was added to `Program.cs` and intentionally **not invoked**.

Included LINQ coverage in the method:
- `.Where(...)` (filter)
- `.OrderByDescending(...)` (descending sort)
- `.Select(...)` (map)
- `.ToList(...)`
- `.ToListAsync(...)`

The method creates sample `RawLead`, `DesignTemplate`, `JsonKeyStat`, and `LeadIntelligence` data only for local/lab usage.