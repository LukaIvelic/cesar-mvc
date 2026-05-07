# EF Skill — Entity Framework Workflow

Use this skill whenever you need to add a new entity, modify an existing one, or generate a migration.

## Project layout

- Entities live in `Features/{FeatureName}/Entities/{EntityName}.cs`
- DbContext is `Data/AppDbContext.cs`
- Migrations live in `Migrations/`
- EF is registered in `Extensions/InfrastructureServiceExtensions.cs`

## Adding a new entity

1. Create `Features/{FeatureName}/Entities/{EntityName}.cs`:
   ```csharp
   using System.ComponentModel.DataAnnotations;

   namespace cesar.Features.{FeatureName}.Entities;

   public class {EntityName}
   {
       [Key]
       public int Id { get; set; }

       // scalar properties ...

       public DateTime ValidFrom { get; set; }
       public DateTime? ValidTo { get; set; }   // soft-delete pattern
   }
   ```

2. Add `DbSet<T>` to `Data/AppDbContext.cs`:
   ```csharp
   public DbSet<{EntityName}> {EntityNamePlural} { get; set; }
   ```

3. Generate and apply the migration:
   ```powershell
   cd C:\Users\iveli\Desktop\cesar
   dotnet ef migrations add Add{EntityName} --context AppDbContext
   dotnet ef database update --context AppDbContext
   ```

## Adding a 1-N relationship

Child entity (many side):
```csharp
[ForeignKey("Parent")]
public int ParentId { get; set; }

public virtual ParentEntity? Parent { get; set; }
```

Parent entity (one side):
```csharp
public virtual ICollection<ChildEntity> Children { get; set; } = new List<ChildEntity>();
```

Then run the migration as above.

## Modifying an existing entity

1. Edit the entity class.
2. Run:
   ```powershell
   dotnet ef migrations add Describe{WhatChanged} --context AppDbContext
   dotnet ef database update --context AppDbContext
   ```

## Notes

- The app auto-applies migrations on startup via `dbContext.Database.Migrate()` in `Program.cs`.
- Connection string key is `Default` in `appsettings.json` (PostgreSQL/Npgsql).
- If the app is running and locking `cesar.exe`, stop it before running EF commands.
- Soft-delete: never use `.Remove()` — set `ValidTo = DateTime.UtcNow` instead.
