using System.ComponentModel.DataAnnotations.Schema;

namespace cesar.Features.DesignTemplates.Entities;

public class DesignTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string HtmlMarkup { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string PlaceholderSchema { get; set; } = "{}";

    public ContentType ContentType { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
