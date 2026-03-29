using cesar.Features.Weather.Entities;

namespace cesar.Features.Weather;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetAllForecastsAsync();
    Task<WeatherForecast?> GetForecastByIdAsync(int id);
    Task AddForecastAsync(WeatherForecast forecast);
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

    public Task AddForecastAsync(WeatherForecast forecast) =>
        _repository.AddAsync(forecast);

    public Task DeleteForecastAsync(int id) =>
        _repository.DeleteAsync(id);
}
