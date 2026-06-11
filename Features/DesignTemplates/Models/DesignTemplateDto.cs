using cesar.Features.DesignTemplates.Entities;

namespace cesar.Features.DesignTemplates.Models;

public class DesignTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string HtmlMarkup { get; set; } = string.Empty;
    public string PlaceholderSchema { get; set; } = "{}";
    public DateTime ValidFrom { get; set; }
    public List<DesignTemplateAttachmentDto> Attachments { get; set; } = [];
}
