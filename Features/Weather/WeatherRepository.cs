using cesar.Data;
using cesar.Features.Weather.Entities;
using Microsoft.EntityFrameworkCore;

namespace cesar.Features.Weather;

public interface IWeatherRepository
{
    Task<IEnumerable<WeatherForecast>> GetAllAsync();
    Task<WeatherForecast?> GetByIdAsync(int id);
    Task AddAsync(WeatherForecast forecast);
    Task DeleteAsync(int id);
}

public class WeatherRepository : IWeatherRepository
{
    private readonly AppDbContext _context;

    public WeatherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WeatherForecast>> GetAllAsync() =>
        await _context.WeatherForecasts.ToListAsync();

    public async Task<WeatherForecast?> GetByIdAsync(int id) =>
        await _context.WeatherForecasts.FindAsync(id);

    public async Task AddAsync(WeatherForecast forecast)
    {
        _context.WeatherForecasts.Add(forecast);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var forecast = await _context.WeatherForecasts.FindAsync(id);
        if (forecast != null)
        {
            _context.WeatherForecasts.Remove(forecast);
            await _context.SaveChangesAsync();
        }
    }
}
