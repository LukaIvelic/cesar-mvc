namespace cesar.Features.Weather.Models;

public class WeatherForecastViewModel
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string TemperatureDisplay { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
