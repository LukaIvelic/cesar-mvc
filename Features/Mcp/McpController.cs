using cesar.Features.GlobalSearch;
using cesar.Features.RawLead;
using cesar.Features.DesignTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace cesar.Features.Mcp;

[ApiController]
[Route("mcp")]
[AllowAnonymous]
public class McpController : ControllerBase
{
    private readonly IGlobalSearchService _globalSearchService;
    private readonly IRawLeadService _rawLeadService;
    private readonly IDesignTemplateService _designTemplateService;

    public McpController(
        IGlobalSearchService globalSearchService,
        IRawLeadService rawLeadService,
        IDesignTemplateService designTemplateService)
    {
        _globalSearchService = globalSearchService;
        _rawLeadService = rawLeadService;
        _designTemplateService = designTemplateService;
    }

    [HttpGet]
    public IActionResult Describe() =>
        Ok(new
        {
            name = "cesar-mcp",
            transport = "streamable-http",
            endpoint = "/mcp",
            tools = new[] { "global_search", "raw_lead_search", "design_template_search" }
        });

    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] JsonElement request)
    {
        var id = request.TryGetProperty("id", out var idProperty) ? idProperty.Clone() : default;
        var method = request.TryGetProperty("method", out var methodProperty)
            ? methodProperty.GetString()
            : null;

        object result = method switch
        {
            "initialize" => new
            {
                protocolVersion = "2024-11-05",
                serverInfo = new { name = "cesar-mcp", version = "1.0.0" },
                capabilities = new { tools = new { } }
            },
            "tools/list" => new
            {
                tools = new[]
                {
                    new
                    {
                        name = "global_search",
                        description = "Search Cesar pages, raw leads, templates, JSON keys and intelligence records.",
                        inputSchema = BuildStringQuerySchema()
                    },
                    new
                    {
                        name = "raw_lead_search",
                        description = "Search active raw lead payloads.",
                        inputSchema = BuildStringQuerySchema()
                    },
                    new
                    {
                        name = "design_template_search",
                        description = "Search active design templates.",
                        inputSchema = BuildStringQuerySchema()
                    }
                }
            },
            "tools/call" => await CallToolAsync(request),
            _ => new { error = $"Unsupported MCP method '{method}'." }
        };

        return Ok(new
        {
            jsonrpc = "2.0",
            id,
            result
        });
    }

    private async Task<object> CallToolAsync(JsonElement request)
    {
        if (!request.TryGetProperty("params", out var parameters) ||
            !parameters.TryGetProperty("name", out var nameProperty))
        {
            return ToolText("Missing tool name.");
        }

        var name = nameProperty.GetString();
        var query = ReadQuery(parameters);

        return name switch
        {
            "global_search" => ToolJson(await _globalSearchService.SearchAsync(query, 8)),
            "raw_lead_search" => ToolJson((await _rawLeadService.SearchActiveAsync(query, 8)).Select(lead => new
            {
                lead.Id,
                lead.SourceSystem,
                lead.ExternalId,
                lead.RawJsonData,
                lead.IngestedAt
            })),
            "design_template_search" => ToolJson((await _designTemplateService.GetAllActiveAsync())
                .Where(template => string.IsNullOrWhiteSpace(query) ||
                    template.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    template.ContentType.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    template.PlaceholderSchema.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(8)
                .Select(template => new
                {
                    template.Id,
                    template.Name,
                    ContentType = template.ContentType.ToString(),
                    template.PlaceholderSchema
                })),
            _ => ToolText($"Unknown tool '{name}'.")
        };
    }

    private static string ReadQuery(JsonElement parameters)
    {
        if (!parameters.TryGetProperty("arguments", out var arguments))
        {
            return string.Empty;
        }

        return arguments.TryGetProperty("query", out var query)
            ? query.GetString() ?? string.Empty
            : string.Empty;
    }

    private static object BuildStringQuerySchema() => new
    {
        type = "object",
        properties = new
        {
            query = new { type = "string", description = "Search query." }
        },
        required = new[] { "query" }
    };

    private static object ToolJson(object payload) =>
        ToolText(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));

    private static object ToolText(string text) => new
    {
        content = new[]
        {
            new { type = "text", text }
        }
    };
}
