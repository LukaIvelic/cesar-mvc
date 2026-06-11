using cesar.Features.Weather.Entities;

namespace cesar.Features.Weather;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetAllForecastsAsync();
    Task<WeatherForecast?> GetForecastByIdAsync(int id);
    Task<WeatherForecast> AddForecastAsync(WeatherForecast forecast);
    Task UpdateForecastAsync(int id, DateOnly date, int temperatureC, string? summary);
    Task DeleteForecastAsync(int id);
}

public class WeatherService : IWeatherService
{
    private readonly IWeatherRepository _repository;

    public WeatherService(IWeatherRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<WeatherForecast>> GetAllForecastsAsync() =>
        _repository.GetAllAsync();

    public Task<WeatherForecast?> GetForecastByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task<WeatherForecast> AddForecastAsync(WeatherForecast forecast)
    {
        await _repository.AddAsync(forecast);
        return forecast;
    }

    public async Task UpdateForecastAsync(int id, DateOnly date, int temperatureC, string? summary)
    {
        var forecast = await _repository.GetByIdAsync(id);
        if (forecast is null) return;

        forecast.Date = date;
        forecast.TemperatureC = temperatureC;
        forecast.Summary = summary;

        await _repository.UpdateAsync(forecast);
    }

    public Task DeleteForecastAsync(int id) =>
        _repository.DeleteAsync(id);
}
