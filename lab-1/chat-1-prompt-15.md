# Chat 1 - Prompt 15

## User

```
$ dotnet ef migrations add InitialCreate
dotnet ef database update
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
Build started...
Build succeeded.
fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
      Failed executing DbCommand (35ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "MigrationId", "ProductVersion"
      FROM "__EFMigrationsHistory"
      ORDER BY "MigrationId";
...
Applying migration '20260329120741_InitialCreate'.
Done.
```

Why exactly is dbcommand failing here?

---

## Claude

Not actually a problem — that's expected behavior on a fresh database.

EF Core always starts by querying `__EFMigrationsHistory` to check which migrations have already been applied. On a brand new database that table doesn't exist yet, so the query fails. EF Core catches that, creates the table, then applies your migrations normally.

The important lines are at the bottom:
```
Applying migration '20260329120741_InitialCreate'.
Done.
```

Migration ran fine. Your NeonDB schema is set up.
