using System.ComponentModel.DataAnnotations;

namespace cesar.Features.JsonKeyStats.Models;

public class EditJsonKeyStatModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Key is required.")]
    public string Key { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Occurrences must be 0 or greater.")]
    public int Occurrences { get; set; }
}
