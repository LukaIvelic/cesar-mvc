using System.ComponentModel.DataAnnotations.Schema;

namespace cesar.Features.RawLead.Entities;

public class RawLead
{
    public int Id { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string RawJsonData { get; set; } = "{}";

    public DateTime IngestedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
