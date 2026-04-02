namespace cesar.Features.RawLead.Models;

public class RawLeadViewModel
{
    public int Id { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public DateTime IngestedAt { get; set; }
    public DateTime ValidFrom { get; set; }
}
