using System.ComponentModel.DataAnnotations;

namespace cesar.Features.Weather.Models;

public class EditWeatherForecastModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Date and time are required.")]
    public DateTime Date { get; set; } = DateTime.Now;

    [Required]
    [Range(-100, 60, ErrorMessage = "Temperature must be between -100 and 60C.")]
    public int TemperatureC { get; set; }

    [MaxLength(100, ErrorMessage = "Summary cannot exceed 100 characters.")]
    public string? Summary { get; set; }
}
