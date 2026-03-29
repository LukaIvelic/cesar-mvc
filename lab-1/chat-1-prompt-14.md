# Chat 1 - Prompt 14

## User

```
$ dotnet ef migrations add InitialCreate
Could not execute because the specified command or file was not found.
Possible reasons for this include:
  * You misspelled a built-in dotnet command.
  * You intended to execute a .NET program, but dotnet-ef does not exist.
  * You intended to run a global tool, but a dotnet-prefixed executable with this name could not be found on the PATH.
```

---

## Claude

`dotnet ef` is a separate global tool install. Run this first:

```bash
dotnet tool install --global dotnet-ef
```

Then retry:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
