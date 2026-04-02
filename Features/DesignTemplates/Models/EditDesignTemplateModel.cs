using cesar.Features.DesignTemplates.Entities;
using System.ComponentModel.DataAnnotations;

namespace cesar.Features.DesignTemplates.Models;

public class EditDesignTemplateModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "HTML markup is required.")]
    public string HtmlMarkup { get; set; } = string.Empty;

    [Required(ErrorMessage = "Placeholder schema is required.")]
    public string PlaceholderSchema { get; set; } = "{}";

    [Required(ErrorMessage = "Content type is required.")]
    public ContentType ContentType { get; set; } = ContentType.Mail;

    [Required(ErrorMessage = "Preview raw JSON is required.")]
    public string PreviewRawJsonData { get; set; } = "{}";

    public List<RawJsonOptionModel> RawJsonOptions { get; set; } = [];
}
