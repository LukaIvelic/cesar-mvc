using cesar.Data;
using cesar.Features.JsonKeyStats;
using cesar.Features.LeadIntelligence;
using cesar.Features.RawLead;
using cesar.Features.DesignTemplates;
using cesar.Features.Weather;
using Microsoft.EntityFrameworkCore;

namespace cesar.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.Equals("YOUR_CONNECTION_STRING_HERE", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Connection string 'Default' is not configured. Set ConnectionStrings:Default " +
                "in appsettings.Development.json or user secrets, for example: " +
                "'Host=localhost;Database=cesar;Username=postgres;Password=your-password'.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IWeatherRepository, WeatherRepository>();
        services.AddScoped<IRawLeadRepository, RawLeadRepository>();
        services.AddScoped<ILeadIntelligenceRepository, LeadIntelligenceRepository>();
        services.AddScoped<IJsonKeyStatRepository, JsonKeyStatRepository>();
        services.AddScoped<IDesignTemplateRepository, DesignTemplateRepository>();

        return services;
    }
}
