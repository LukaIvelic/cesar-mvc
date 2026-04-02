using System.ComponentModel.DataAnnotations;

namespace cesar.Features.RawLead.Models;

public class EditRawLeadModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Source system is required.")]
    public string SourceSystem { get; set; } = string.Empty;

    [Required(ErrorMessage = "External ID is required.")]
    public string ExternalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "JSON data is required.")]
    public string RawJsonData { get; set; } = string.Empty;
}
