using cesar.Features.DesignTemplates.Entities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace cesar.Features.DesignTemplates;

public interface IDesignTemplateService
{
    Task<IEnumerable<DesignTemplate>> GetAllActiveAsync();
    Task<DesignTemplate?> GetByIdAsync(int id);
    Task CreateAsync(string name, ContentType contentType, string htmlMarkup, string placeholderSchema);
    Task UpdateAsync(int id, string name, ContentType contentType, string htmlMarkup, string placeholderSchema);
    Task SoftDeleteAsync(int id);
    string RenderMarkup(string htmlMarkup, string rawJsonData);
}

public class DesignTemplateService : IDesignTemplateService
{
    private static readonly Regex PlaceholderRegex = new(@"{{\s*([a-zA-Z0-9_.]+)\s*}}", RegexOptions.Compiled);
    private readonly IDesignTemplateRepository _repository;

    public DesignTemplateService(IDesignTemplateRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<DesignTemplate>> GetAllActiveAsync() =>
        _repository.GetAllActiveAsync();

    public Task<DesignTemplate?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task CreateAsync(string name, ContentType contentType, string htmlMarkup, string placeholderSchema)
    {
        await _repository.AddAsync(new DesignTemplate
        {
            Name = name,
            ContentType = contentType,
            HtmlMarkup = htmlMarkup,
            PlaceholderSchema = placeholderSchema,
            ValidFrom = DateTime.UtcNow
        });
    }

    public async Task UpdateAsync(int id, string name, ContentType contentType, string htmlMarkup, string placeholderSchema)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return;

        entity.Name = name;
        entity.ContentType = contentType;
        entity.HtmlMarkup = htmlMarkup;
        entity.PlaceholderSchema = placeholderSchema;

        await _repository.UpdateAsync(entity);
    }

    public Task SoftDeleteAsync(int id) =>
        _repository.SoftDeleteAsync(id);

    public string RenderMarkup(string htmlMarkup, string rawJsonData)
    {
        if (string.IsNullOrWhiteSpace(htmlMarkup)) return string.Empty;
        if (string.IsNullOrWhiteSpace(rawJsonData)) return htmlMarkup;

        try
        {
            using var document = JsonDocument.Parse(rawJsonData);
            return PlaceholderRegex.Replace(htmlMarkup, m =>
            {
                var path = m.Groups[1].Value;
                var value = ResolvePath(document.RootElement, path);
                return value ?? m.Value;
            });
        }
        catch
        {
            return htmlMarkup;
        }
    }

    private static string? ResolvePath(JsonElement root, string path)
    {
        var current = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (!TryGetPropertyIgnoreCase(current, segment, out var next))
            {
                return null;
            }

            current = next;
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            _ => current.GetRawText()
        };
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value)) return true;

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
}
