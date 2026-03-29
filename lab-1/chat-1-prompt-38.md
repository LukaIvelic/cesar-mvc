# Chat 1 - Prompt 38

## User

Why do I have to write npm run build:css everytime? Integrate it with dotnet build pipeline and tell me exactly how you did it

---

## Claude

Added a MSBuild `Target` to `cesar.csproj`:

```xml
<Target Name="BuildTailwind" BeforeTargets="Build">
  <Exec Command="npm run build:css" />
</Target>
```

How it works:
- `.csproj` is an MSBuild file — the same build system that runs when you do `dotnet build` or `dotnet run`
- `<Target>` defines a named build step ("BuildTailwind")
- `BeforeTargets="Build"` tells MSBuild to run this step before the main C# compilation
- `<Exec Command="...">` runs a shell command — in this case `npm run build:css`
- So every `dotnet build` / `dotnet run` now automatically rebuilds `site.css` first
