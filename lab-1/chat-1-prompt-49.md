# Chat 1 - Prompt 49

## User

PostgresException: 42P01: relation "RawLeads" does not exist

---

## Claude

Migration was never applied. Table doesn't exist in NeonDB yet.

```bash
dotnet ef migrations add AddRawLead
dotnet ef database update
```

If migration file already exists, just run `dotnet ef database update`.
