using cesar.Features.DesignTemplates.Entities;
using RawLeadEntity = cesar.Features.RawLead.Entities.RawLead;
using System.ComponentModel.DataAnnotations;

namespace cesar.Features.AiTemplateGenerator.Models;

public class AiTemplateGeneratorViewModel
{
    [Required(ErrorMessage = "Describe what AI should generate.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Prompt must be between 10 and 4000 characters.")]
    public string GenerationPrompt { get; set; } =
        "Generate a personalized follow-up email for the selected lead data.";

    [Required(ErrorMessage = "Content type is required.")]
    public ContentType ContentType { get; set; } = ContentType.Mail;

    [StringLength(80, ErrorMessage = "Language must be 80 characters or fewer.")]
    public string OutputLanguage { get; set; } = "Croatian";

    [StringLength(200, ErrorMessage = "Tone must be 200 characters or fewer.")]
    public string Tone { get; set; } = string.Empty;

    public List<int> SelectedRawLeadIds { get; set; } = [];

    public List<string> SelectedJsonPaths { get; set; } = [];

    public List<AiRawLeadOptionModel> RawLeadOptions { get; set; } = [];

    public AiGeneratedTemplateResult? Result { get; set; }
}

public class AiRawLeadOptionModel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string RawJsonData { get; set; } = "{}";
}

public sealed class AiGeneratedTemplateResult
{
    public string Name { get; init; } = string.Empty;
    public ContentType ContentType { get; init; }
    public string Content { get; init; } = string.Empty;
    public string PlaceholderSchema { get; init; } = "{}";
    public string Notes { get; init; } = string.Empty;
}

public sealed class SaveGeneratedTemplateModel
{
    [Required(ErrorMessage = "Template name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content type is required.")]
    public ContentType ContentType { get; set; } = ContentType.Mail;

    [Required(ErrorMessage = "Generated content is required.")]
    public string HtmlMarkup { get; set; } = string.Empty;

    [Required(ErrorMessage = "Placeholder schema is required.")]
    public string PlaceholderSchema { get; set; } = "{}";
}

public sealed record AiTemplateGenerationRequest(
    string GenerationPrompt,
    ContentType ContentType,
    string OutputLanguage,
    string Tone,
    IReadOnlyCollection<RawLeadEntity> RawLeads,
    IReadOnlyCollection<string> SelectedJsonPaths);
