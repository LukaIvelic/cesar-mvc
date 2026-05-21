using System.ComponentModel.DataAnnotations;

namespace cesar.Features.LeadIntelligence.Models;

public class EditLeadIntelligenceModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Lead is required.")]
    public int LeadId { get; set; }

    public string LeadDisplayName { get; set; } = string.Empty;

    [Required]
    public string ContentHash { get; set; } = string.Empty;

    [Required]
    [Range(0.0, 1.0, ErrorMessage = "Must be between 0 and 1.")]
    public double FamiliarityIndex { get; set; }

    [Required]
    [Range(0.0, 1.0, ErrorMessage = "Must be between 0 and 1.")]
    public double DataDensityScore { get; set; }
}
