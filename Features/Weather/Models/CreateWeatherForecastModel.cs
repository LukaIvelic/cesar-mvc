using System.ComponentModel.DataAnnotations;

namespace cesar.Features.Weather.Models;

public class CreateWeatherForecastModel
{
    [Required]
    public DateOnly Date { get; set; }

    [Required]
    [Range(-100, 60, ErrorMessage = "Temperature must be between -100 and 60°C.")]
    public int TemperatureC { get; set; }

    [MaxLength(100, ErrorMessage = "Summary cannot exceed 100 characters.")]
    public string? Summary { get; set; }
}
