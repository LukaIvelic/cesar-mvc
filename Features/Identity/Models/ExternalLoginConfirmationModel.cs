using System.ComponentModel.DataAnnotations;

namespace cesar.Features.Identity.Models;

public class ExternalLoginConfirmationModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB can contain only numbers.")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG can contain only numbers.")]
    public string JMBG { get; set; } = string.Empty;

    public string? LoginProvider { get; set; }

    public string? ReturnUrl { get; set; }
}
