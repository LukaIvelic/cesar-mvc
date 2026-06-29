using cesar.Features.DesignTemplates;
using cesar.Features.JsonKeyStats;
using cesar.Features.LeadIntelligence;
using cesar.Features.RawLead;

namespace cesar.Features.GlobalSearch;

public interface IGlobalSearchService
{
    Task<GlobalSearchResultSet> SearchAsync(string? query, int take = 8);
}

public sealed class GlobalSearchService : IGlobalSearchService
{
    private static readonly IReadOnlyList<GlobalSearchResult> Pages =
    [
        new("page", "Overview", "Dashboard and pipeline overview", "/", "Home"),
        new("page", "Raw Leads", "Source JSON payloads", "/leads", "Data"),
        new("page", "Intelligence", "Lead scoring and AI analysis", "/LeadIntelligence", "Data"),
        new("page", "Templates", "Reusable output templates", "/DesignTemplate", "Create"),
        new("page", "AI Generator", "Generate content from raw leads", "/ai-template-generator", "Create"),
        new("page", "JSON Key Stats", "Tracked JSON fields", "/JsonKeyStat", "Data"),
        new("page", "Weather Demo", "Sample CRUD module", "/WeatherForecast", "System"),
        new("page", "Privacy", "Application privacy page", "/Home/Privacy", "System")
    ];

    private readonly IRawLeadService _rawLeadService;
    private readonly IDesignTemplateService _designTemplateService;
    private readonly IJsonKeyStatService _jsonKeyStatService;
    private readonly ILeadIntelligenceService _leadIntelligenceService;

    public GlobalSearchService(
        IRawLeadService rawLeadService,
        IDesignTemplateService designTemplateService,
        IJsonKeyStatService jsonKeyStatService,
        ILeadIntelligenceService leadIntelligenceService)
    {
        _rawLeadService = rawLeadService;
        _designTemplateService = designTemplateService;
        _jsonKeyStatService = jsonKeyStatService;
        _leadIntelligenceService = leadIntelligenceService;
    }

    public async Task<GlobalSearchResultSet> SearchAsync(string? query, int take = 8)
    {
        var term = query?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(term))
        {
            return new GlobalSearchResultSet(term, Pages.Take(take).ToList());
        }

        var results = new List<GlobalSearchResult>();

        results.AddRange(Pages
            .Where(page => Matches(term, page.Title, page.Description, page.Group))
            .Take(take));

        var rawLeads = await _rawLeadService.SearchActiveAsync(term, take);
        results.AddRange(rawLeads.Select(lead =>
            new GlobalSearchResult(
                "raw-lead",
                $"#{lead.Id} {lead.ExternalId}",
                $"{lead.SourceSystem} raw JSON lead",
                $"/leads/{lead.Id}",
                "Raw Leads")));

        var templates = await _designTemplateService.GetAllActiveAsync();
        results.AddRange(templates
            .Where(template => Matches(term, template.Name, template.ContentType.ToString(), template.PlaceholderSchema))
            .Take(take)
            .Select(template =>
                new GlobalSearchResult(
                    "template",
                    template.Name,
                    $"{template.ContentType} template",
                    $"/DesignTemplate/Preview/{template.Id}",
                    "Templates")));

        var keyStats = await _jsonKeyStatService.GetAllActiveAsync();
        results.AddRange(keyStats
            .Where(stat => Matches(term, stat.Key))
            .Take(take)
            .Select(stat =>
                new GlobalSearchResult(
                    "json-key",
                    stat.Key,
                    $"{stat.Occurrences} occurrences",
                    "/JsonKeyStat",
                    "JSON Key Stats")));

        var intelligence = await _leadIntelligenceService.GetAllActiveAsync();
        results.AddRange(intelligence
            .Where(record => Matches(term, record.Id.ToString(), record.LeadId.ToString(), record.ContentHash))
            .Take(take)
            .Select(record =>
                new GlobalSearchResult(
                    "intelligence",
                    $"Intelligence #{record.Id}",
                    $"Lead #{record.LeadId} hash {record.ContentHash}",
                    "/LeadIntelligence",
                    "Intelligence")));

        return new GlobalSearchResultSet(
            term,
            results
                .DistinctBy(result => $"{result.Type}:{result.Url}")
                .Take(take * 5)
                .ToList());
    }

    private static bool Matches(string term, params string?[] values) =>
        values.Any(value =>
            !string.IsNullOrWhiteSpace(value)
            && value.Contains(term, StringComparison.OrdinalIgnoreCase));
}

public sealed record GlobalSearchResultSet(string Query, IReadOnlyList<GlobalSearchResult> Results);

public sealed record GlobalSearchResult(
    string Type,
    string Title,
    string Description,
    string Url,
    string Group);
