using System.ComponentModel.DataAnnotations;

namespace cesar.Features.Weather.Models;

public class CreateWeatherForecastDto
{
    [Required]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Range(-100, 60)]
    public int TemperatureC { get; set; }

    [MaxLength(100)]
    public string? Summary { get; set; }
}
