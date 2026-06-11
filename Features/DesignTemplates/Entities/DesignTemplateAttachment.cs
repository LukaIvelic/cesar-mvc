using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cesar.Features.DesignTemplates.Entities;

public class DesignTemplateAttachment
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(DesignTemplate))]
    public int DesignTemplateId { get; set; }

    public DesignTemplate? DesignTemplate { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
