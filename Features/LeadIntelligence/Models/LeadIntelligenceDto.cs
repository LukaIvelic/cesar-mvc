namespace cesar.Features.LeadIntelligence.Models;

public class LeadIntelligenceDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public double FamiliarityIndex { get; set; }
    public double DataDensityScore { get; set; }
    public DateTime LastAnalyzedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public RawLeadSummaryDto? Lead { get; set; }
}

public class RawLeadSummaryDto
{
    public int Id { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
}
