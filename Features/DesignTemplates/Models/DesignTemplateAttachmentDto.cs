namespace cesar.Features.DesignTemplates.Models;

public class DesignTemplateAttachmentDto
{
    public int Id { get; set; }
    public int DesignTemplateId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
