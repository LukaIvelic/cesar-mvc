using System.ComponentModel.DataAnnotations;
using cesar.Features.DesignTemplates.Entities;

namespace cesar.Features.DesignTemplates.Models;

public class CreateDesignTemplateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public ContentType ContentType { get; set; } = ContentType.Mail;

    [Required]
    public string HtmlMarkup { get; set; } = string.Empty;

    [Required]
    public string PlaceholderSchema { get; set; } = "{}";
}
