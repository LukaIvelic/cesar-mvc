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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IWeatherRepository, WeatherRepository>();
        services.AddScoped<IRawLeadRepository, RawLeadRepository>();
        services.AddScoped<ILeadIntelligenceRepository, LeadIntelligenceRepository>();
        services.AddScoped<IJsonKeyStatRepository, JsonKeyStatRepository>();
        services.AddScoped<IDesignTemplateRepository, DesignTemplateRepository>();

        return services;
    }
}
