# Chat 1 - Prompt 46

## User (Croatian)

Why use automatic properties instead of fields? There are differences in how C# treats variables and properties. Rules:
- A class must not contain public fields
- A class should contain properties (public or protected, rarely private)
- Use automatic property where possible

Do we follow these rules?

---

## Claude

Yes, rules are followed:

- Entities and Models: all automatic properties (`{ get; set; }`), no fields
- Services/Repository/Controller: `private readonly` fields for DI-injected dependencies only

The `private readonly` fields are the accepted exception — they're set once in constructor and never change. Making them private properties would work but is unnecessarily verbose. Universal C# convention for DI dependencies.

Public API → automatic properties. DI dependencies → `private readonly` fields. No public fields anywhere.
