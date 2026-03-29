using cesar.Features.Weather.Entities;
using Microsoft.EntityFrameworkCore;

namespace cesar.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }
}
