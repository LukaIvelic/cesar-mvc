using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace cesar.Features.LeadIntelligence;

public sealed class LeadIntelligenceAnalysisResult
{
    public double FamiliarityIndex { get; init; }
    public double DataDensityScore { get; init; }
}

public interface ILeadIntelligenceAnalyzer
{
    Task<LeadIntelligenceAnalysisResult> AnalyzeAsync(string rawJsonData, CancellationToken cancellationToken = default);
}

public class OpenAiLeadIntelligenceAnalyzer : ILeadIntelligenceAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenAiLeadIntelligenceAnalyzer(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<LeadIntelligenceAnalysisResult> AnalyzeAsync(
        string rawJsonData,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured in appsettings.Development.json.");
        }

        var model = _configuration["OpenAI:Model"];
        if (string.IsNullOrWhiteSpace(model))
        {
            model = "gpt-5.4-mini";
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model,
            input = $$"""
            Analyze this raw lead JSON and return only valid JSON with these numeric fields:
            familiarityIndex: 0 to 1, where higher means the lead looks familiar and complete.
            dataDensityScore: 0 to 1, where higher means the JSON contains dense useful lead data.

            Raw lead JSON:
            {{rawJsonData}}
            """,
            max_output_tokens = 200
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("OpenAI API call failed while analyzing the lead.");
        }

        var outputText = ExtractOutputText(responseJson);
        return ParseAnalysisResult(outputText);
    }

    private static string ExtractOutputText(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        if (root.TryGetProperty("output_text", out var outputText))
        {
            return outputText.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("type", out var type)
                    && type.GetString() == "output_text"
                    && contentItem.TryGetProperty("text", out var text))
                {
                    return text.GetString() ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }

    private static LeadIntelligenceAnalysisResult ParseAnalysisResult(string outputText)
    {
        var json = outputText.Trim();
        if (json.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            json = json[7..].Trim();
        }

        if (json.StartsWith("```", StringComparison.Ordinal))
        {
            json = json[3..].Trim();
        }

        if (json.EndsWith("```", StringComparison.Ordinal))
        {
            json = json[..^3].Trim();
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new LeadIntelligenceAnalysisResult
        {
            FamiliarityIndex = Clamp(ReadDouble(root, "familiarityIndex", "familiarity_index")),
            DataDensityScore = Clamp(ReadDouble(root, "dataDensityScore", "data_density_score"))
        };
    }

    private static double ReadDouble(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var property) && property.TryGetDouble(out var value))
            {
                return value;
            }
        }

        throw new InvalidOperationException("OpenAI response did not contain the expected score fields.");
    }

    private static double Clamp(double value) => Math.Max(0, Math.Min(1, value));
}
