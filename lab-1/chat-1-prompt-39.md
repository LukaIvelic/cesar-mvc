# Chat 1 - Prompt 39

## User

Can't the th elements be simplified? Have an array of th like [date, temp, summary, actions] and then in a foreach loop render them

---

## Claude

Used `@{ }` block at the top of the view to declare a C# array, then looped over it with `@foreach` to render the headers:

```razor
@{
    var columns = new[] { "Date", "Temperature", "Summary", "Actions" };
}
...
@foreach (var column in columns)
{
    <th class="...">@column</th>
}
```

`@{ }` is a Razor code block — any C# you write inside it is executed server-side before the view renders.
