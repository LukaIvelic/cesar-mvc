using cesar.Features.JsonKeyStats;
using System.Text.Json;

namespace cesar.Features.RawLead;

using RawLead = global::cesar.Features.RawLead.Entities.RawLead;

public interface IRawLeadService
{
    Task<IEnumerable<RawLead>> GetAllActiveAsync();
    Task<RawLead?> GetByIdAsync(int id);
    Task CreateAsync(string sourceSystem, string externalId, string rawJson);
    Task CreateBulkAsync(IEnumerable<(string SourceSystem, string ExternalId, string RawJson)> leads);
    Task UpdateAsync(int id, string sourceSystem, string externalId, string rawJson);
    Task SoftDeleteAsync(int id);
}

public class RawLeadService : IRawLeadService
{
    private readonly IRawLeadRepository _repository;
    private readonly IJsonKeyStatService _keyStatService;

    public RawLeadService(IRawLeadRepository repository, IJsonKeyStatService keyStatService)
    {
        _repository = repository;
        _keyStatService = keyStatService;
    }

    public Task<IEnumerable<RawLead>> GetAllActiveAsync() =>
        _repository.GetAllActiveAsync();

    public Task<RawLead?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public async Task CreateAsync(string sourceSystem, string externalId, string rawJson)
    {
        var now = DateTime.UtcNow;
        await _repository.AddAsync(new RawLead
        {
            SourceSystem = sourceSystem,
            ExternalId = externalId,
            RawJsonData = rawJson,
            IngestedAt = now,
            ValidFrom = now
        });

        var keys = ExtractKeys(rawJson);
        if (keys.Any())
            await _keyStatService.TrackKeysAsync(keys);
    }

    public async Task CreateBulkAsync(IEnumerable<(string SourceSystem, string ExternalId, string RawJson)> leads)
    {
        var now = DateTime.UtcNow;
        var entities = leads.Select(l => new RawLead
        {
            SourceSystem = l.SourceSystem,
            ExternalId = l.ExternalId,
            RawJsonData = l.RawJson,
            IngestedAt = now,
            ValidFrom = now
        }).ToList();

        await _repository.AddRangeAsync(entities);

        var allKeys = entities
            .SelectMany(e => ExtractKeys(e.RawJsonData))
            .Distinct();

        if (allKeys.Any())
            await _keyStatService.TrackKeysAsync(allKeys);
    }

    public async Task UpdateAsync(int id, string sourceSystem, string externalId, string rawJson)
    {
        var lead = await _repository.GetByIdAsync(id);
        if (lead is null) return;

        lead.SourceSystem = sourceSystem;
        lead.ExternalId = externalId;
        lead.RawJsonData = rawJson;
        await _repository.UpdateAsync(lead);
    }

    public Task SoftDeleteAsync(int id) =>
        _repository.SoftDeleteAsync(id);

    private static IEnumerable<string> ExtractKeys(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
                return doc.RootElement.EnumerateObject().Select(p => p.Name).ToList();
        }
        catch { }
        return [];
    }
}
