using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RawLeadEntity = cesar.Features.RawLead.Entities.RawLead;

namespace cesar.Features.LeadIntelligence.Entities;

public class LeadIntelligence
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Lead")]
    public int LeadId { get; set; }

    public virtual RawLeadEntity? Lead { get; set; }

    public string ContentHash { get; set; } = string.Empty;
    public double FamiliarityIndex { get; set; }
    public double DataDensityScore { get; set; }
    public DateTime LastAnalyzedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
