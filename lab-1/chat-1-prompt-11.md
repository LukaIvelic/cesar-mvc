# Chat 1 - Prompt 11

## User

Back to ASP.NET. So you're saying I need to register every single service, dbcontext and repository in Program.cs? That can't possibly follow design patterns... Search how to register everything in the most optimal way possible. Just answer this: wouldn't it be easier to just create a separate file where you register all servicer and whatnot and then insert it into Program.cs?

---

## Claude

Yes — that's exactly the standard pattern. Extension methods on `IServiceCollection`.

### The pattern: Extension Methods

Create a static class per layer, each with an extension method:

```csharp
// Extensions/ApplicationServiceExtensions.cs
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}

// Extensions/InfrastructureServiceExtensions.cs
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("Default")));
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
```

Then Program.cs stays clean:

```csharp
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
```

This is the official Microsoft-recommended pattern — used in all Clean Architecture templates.
