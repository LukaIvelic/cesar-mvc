using System.ComponentModel.DataAnnotations;

namespace cesar.Features.Identity.Models;

public class RegisterModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB can contain only numbers.")]
    public string OIB { get; set; } = string.Empty;

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG can contain only numbers.")]
    public string JMBG { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
