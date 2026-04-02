# Chat 1 - Prompt 50

## User

Build failed — dotnet ef migrations add AddRawLead also fails

---

## Claude

No C# errors. The build fails only because cesar.exe is locked by process 26808 (app still running).

Fix:
```bash
taskkill /PID 26808 /F
dotnet ef migrations add AddRawLead && dotnet ef database update
```
