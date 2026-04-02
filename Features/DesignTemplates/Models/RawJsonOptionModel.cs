namespace cesar.Features.DesignTemplates.Models;

public class RawJsonOptionModel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string RawJsonData { get; set; } = "{}";
}
