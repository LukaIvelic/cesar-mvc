using cesar.Features.JsonKeyStats;
using cesar.Features.LeadIntelligence;
using cesar.Features.RawLead;
using cesar.Features.DesignTemplates;
using cesar.Features.Weather;

namespace cesar.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IWeatherService, WeatherService>();
        services.AddScoped<IRawLeadService, RawLeadService>();
        services.AddScoped<ILeadIntelligenceService, LeadIntelligenceService>();
        services.AddScoped<IJsonKeyStatService, JsonKeyStatService>();
        services.AddScoped<IDesignTemplateService, DesignTemplateService>();
        return services;
    }
}
