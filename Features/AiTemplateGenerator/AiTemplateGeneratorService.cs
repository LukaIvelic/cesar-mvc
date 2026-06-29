using cesar.Features.AiTemplateGenerator.Models;
using cesar.Features.DesignTemplates.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace cesar.Features.AiTemplateGenerator;

public interface IAiTemplateGeneratorService
{
    Task<AiGeneratedTemplateResult> GenerateAsync(
        AiTemplateGenerationRequest generationRequest,
        CancellationToken cancellationToken = default);
}

public class OpenAiTemplateGeneratorService : IAiTemplateGeneratorService
{
    private static readonly JsonSerializerOptions IndentedJsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenAiTemplateGeneratorService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AiGeneratedTemplateResult> GenerateAsync(
        AiTemplateGenerationRequest generationRequest,
        CancellationToken cancellationToken = default)
    {
        if (generationRequest.RawLeads.Count == 0)
        {
            throw new InvalidOperationException("Select at least one active raw lead.");
        }

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Set OpenAI:ApiKey with user secrets or the OpenAI__ApiKey environment variable.");
        }

        var model = _configuration["OpenAI:TemplateModel"];
        if (string.IsNullOrWhiteSpace(model))
        {
            model = _configuration["OpenAI:Model"];
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            model = "gpt-5.4-mini";
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model,
            input = BuildPrompt(generationRequest),
            max_output_tokens = 3500
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI API call failed while generating the template ({(int)response.StatusCode}).");
        }

        var outputText = ExtractOutputText(responseJson);
        if (string.IsNullOrWhiteSpace(outputText))
        {
            throw new InvalidOperationException("OpenAI response did not contain generated text.");
        }

        return ParseGenerationResult(
            outputText,
            generationRequest.ContentType,
            BuildFallbackPlaceholderSchema(generationRequest));
    }

    private static string BuildPrompt(AiTemplateGenerationRequest generationRequest)
    {
        var selectedPaths = NormalizePaths(generationRequest.SelectedJsonPaths);
        var contextJson = BuildLeadContextJson(generationRequest, selectedPaths);
        var tone = string.IsNullOrWhiteSpace(generationRequest.Tone)
            ? "Use a professional, direct tone."
            : generationRequest.Tone.Trim();

        return string.Join(Environment.NewLine, [
            "You are Cesar's AI template generator for raw lead data.",
            "Generate one high-quality, ready-to-use result for the requested content type.",
            "Return only valid JSON with no Markdown fences and this shape:",
            "{",
            "  \"name\": \"short reusable template name\",",
            "  \"content\": \"generated email, SMS, message, slogan or self-contained HTML snippet\",",
            "  \"placeholderSchema\": { \"field\": \"sample value\" },",
            "  \"notes\": \"short implementation note\"",
            "}",
            "Rules:",
            "- Honor the user's request before adding your own assumptions.",
            "- Use the selected database lead data as the source context.",
            "- When reusable lead-specific values are useful, use placeholders such as {{fullName}} or {{company.name}} that match selected JSON paths.",
            "- If the user asks for a personalized one-off output, use the actual values from the lead data.",
            "- For HTML output, return a complete self-contained snippet. Inline CSS and small isolated JavaScript are allowed; external dependencies are not.",
            "- For SMS output, keep it concise and practical.",
            "- Do not include explanations outside the JSON object.",
            string.Empty,
            $"Requested content type: {generationRequest.ContentType}",
            $"Requested language: {generationRequest.OutputLanguage.Trim()}",
            $"Tone or constraints: {tone}",
            string.Empty,
            "User request:",
            generationRequest.GenerationPrompt.Trim(),
            string.Empty,
            "Selected raw lead data from the database:",
            contextJson
        ]);
    }

    private static string BuildLeadContextJson(
        AiTemplateGenerationRequest generationRequest,
        IReadOnlyCollection<string> selectedPaths)
    {
        var leads = generationRequest.RawLeads.Select(lead =>
        {
            Dictionary<string, string?>? selectedData = null;

            if (selectedPaths.Count > 0)
            {
                selectedData = ExtractSelectedData(lead.RawJsonData, selectedPaths);
            }

            return new
            {
                lead.Id,
                lead.SourceSystem,
                lead.ExternalId,
                selectedData,
                rawJsonData = selectedPaths.Count == 0 ? lead.RawJsonData : null
            };
        });

        var payload = new
        {
            selectedJsonPaths = selectedPaths,
            leads
        };

        return JsonSerializer.Serialize(payload, IndentedJsonOptions);
    }

    private static Dictionary<string, string?> ExtractSelectedData(
        string rawJsonData,
        IReadOnlyCollection<string> selectedPaths)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var document = JsonDocument.Parse(rawJsonData);
            foreach (var path in selectedPaths)
            {
                if (TryResolvePath(document.RootElement, path, out var value))
                {
                    values[path] = ToPromptValue(value);
                }
            }
        }
        catch
        {
            values["_rawJsonData"] = rawJsonData;
        }

        return values;
    }

    private static string BuildFallbackPlaceholderSchema(AiTemplateGenerationRequest generationRequest)
    {
        var firstLead = generationRequest.RawLeads.FirstOrDefault();
        if (firstLead is null)
        {
            return "{}";
        }

        var selectedPaths = NormalizePaths(generationRequest.SelectedJsonPaths);
        if (selectedPaths.Count == 0)
        {
            return TryPrettyJson(firstLead.RawJsonData, out var rawJsonSchema)
                ? rawJsonSchema
                : "{}";
        }

        var values = ExtractSelectedData(firstLead.RawJsonData, selectedPaths);
        var schema = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var (path, value) in values)
        {
            SetNestedValue(schema, path, value);
        }

        return JsonSerializer.Serialize(schema, IndentedJsonOptions);
    }

    private static List<string> NormalizePaths(IEnumerable<string> paths) =>
        paths
            .Select(path => path.Trim())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static void SetNestedValue(Dictionary<string, object?> root, string path, string? value)
    {
        var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return;
        }

        var current = root;
        foreach (var segment in segments[..^1])
        {
            if (!current.TryGetValue(segment, out var existing) ||
                existing is not Dictionary<string, object?> next)
            {
                next = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                current[segment] = next;
            }

            current = next;
        }

        current[segments[^1]] = value;
    }

    private static bool TryResolvePath(JsonElement root, string path, out JsonElement value)
    {
        var current = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (!TryGetPropertyIgnoreCase(current, segment, out current))
                {
                    value = default;
                    return false;
                }

                continue;
            }

            if (current.ValueKind == JsonValueKind.Array && int.TryParse(segment, out var index))
            {
                var arrayLength = current.GetArrayLength();
                if (index < 0 || index >= arrayLength)
                {
                    value = default;
                    return false;
                }

                current = current.EnumerateArray().ElementAt(index);
                continue;
            }

            value = default;
            return false;
        }

        value = current;
        return true;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value))
        {
            return true;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string? ToPromptValue(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => value.GetRawText()
        };

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

    private static AiGeneratedTemplateResult ParseGenerationResult(
        string outputText,
            ContentType contentType,
        string fallbackPlaceholderSchema)
    {
        var json = StripCodeFence(outputText);

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var content = ReadString(root, "content", "html", "body", "message", "text");
            if (string.IsNullOrWhiteSpace(content))
            {
                return BuildRawTextResult(json, contentType, fallbackPlaceholderSchema);
            }

            var placeholderSchema = ReadPlaceholderSchema(root, fallbackPlaceholderSchema);

            return new AiGeneratedTemplateResult
            {
                Name = ReadString(root, "name", "title") ?? BuildDefaultName(contentType),
                ContentType = contentType,
                Content = content.Trim(),
                PlaceholderSchema = placeholderSchema,
                Notes = ReadString(root, "notes", "note", "rationale") ?? string.Empty
            };
        }
        catch
        {
            return BuildRawTextResult(json, contentType, fallbackPlaceholderSchema);
        }
    }

    private static AiGeneratedTemplateResult BuildRawTextResult(
        string content,
        ContentType contentType,
        string fallbackPlaceholderSchema) =>
        new()
        {
            Name = BuildDefaultName(contentType),
            ContentType = contentType,
            Content = content.Trim(),
            PlaceholderSchema = fallbackPlaceholderSchema,
            Notes = "The model returned plain text, so the generator used it as the template content."
        };

    private static string StripCodeFence(string text)
    {
        var value = text.Trim();

        if (value.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            value = value[7..].Trim();
        }

        if (value.StartsWith("```", StringComparison.Ordinal))
        {
            value = value[3..].Trim();
        }

        if (value.EndsWith("```", StringComparison.Ordinal))
        {
            value = value[..^3].Trim();
        }

        return value;
    }

    private static string? ReadString(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var property))
            {
                return property.ValueKind == JsonValueKind.String
                    ? property.GetString()
                    : property.GetRawText();
            }
        }

        return null;
    }

    private static string ReadPlaceholderSchema(JsonElement root, string fallbackPlaceholderSchema)
    {
        if (!root.TryGetProperty("placeholderSchema", out var property) &&
            !root.TryGetProperty("placeholder_schema", out property))
        {
            return fallbackPlaceholderSchema;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var schema = property.GetString();
            return !string.IsNullOrWhiteSpace(schema) && TryPrettyJson(schema, out var pretty)
                ? pretty
                : fallbackPlaceholderSchema;
        }

        return TryPrettyJson(property.GetRawText(), out var propertyJson)
            ? propertyJson
            : fallbackPlaceholderSchema;
    }

    private static bool TryPrettyJson(string json, out string prettyJson)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            prettyJson = JsonSerializer.Serialize(document.RootElement, IndentedJsonOptions);
            return true;
        }
        catch
        {
            prettyJson = string.Empty;
            return false;
        }
    }

    private static string BuildDefaultName(ContentType contentType) =>
        $"AI {contentType} Template {DateTime.UtcNow:yyyyMMdd-HHmm}";
}
