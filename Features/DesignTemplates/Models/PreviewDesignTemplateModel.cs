using cesar.Features.DesignTemplates.Entities;

namespace cesar.Features.DesignTemplates.Models;

public class PreviewDesignTemplateModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string HtmlMarkup { get; set; } = string.Empty;
    public string PlaceholderSchema { get; set; } = "{}";
    public string PreviewRawJsonData { get; set; } = "{}";
    public string RenderedHtml { get; set; } = string.Empty;
    public List<RawJsonOptionModel> RawJsonOptions { get; set; } = [];
}
