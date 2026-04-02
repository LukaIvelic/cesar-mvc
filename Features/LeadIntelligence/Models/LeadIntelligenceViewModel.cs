namespace cesar.Features.LeadIntelligence.Models;

public class LeadIntelligenceViewModel
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public double FamiliarityIndex { get; set; }
    public double DataDensityScore { get; set; }
    public DateTime LastAnalyzedAt { get; set; }
    public DateTime ValidFrom { get; set; }
}
