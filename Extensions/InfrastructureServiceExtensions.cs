using cesar.Data;
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

        return services;
    }
}
