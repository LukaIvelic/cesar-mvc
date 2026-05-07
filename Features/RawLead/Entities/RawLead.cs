using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LeadIntelligenceEntity = cesar.Features.LeadIntelligence.Entities.LeadIntelligence;

namespace cesar.Features.RawLead.Entities;

public class RawLead
{
    [Key]
    public int Id { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string RawJsonData { get; set; } = "{}";

    public DateTime IngestedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public virtual ICollection<LeadIntelligenceEntity> Intelligences { get; set; } = new List<LeadIntelligenceEntity>();
}
