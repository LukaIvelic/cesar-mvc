using System.Security.Cryptography;
using System.Text;
using cesar.Features.RawLead;

namespace cesar.Features.LeadIntelligence;

using LeadIntelligence = global::cesar.Features.LeadIntelligence.Entities.LeadIntelligence;

public interface ILeadIntelligenceService
{
    Task<IEnumerable<LeadIntelligence>> GetAllActiveAsync();
    Task<LeadIntelligence?> GetByIdAsync(int id);
    Task<LeadIntelligence?> GetByContentHashAsync(string contentHash);
    Task CreateAsync(int leadId, string contentHash, double familiarityIndex, double dataDensityScore);
    Task<LeadIntelligence> AnalyzeLeadAsync(int leadId, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, int leadId, string contentHash, double familiarityIndex, double dataDensityScore);
    Task SoftDeleteAsync(int id);
    string ComputeSha256(string content);
}

public class LeadIntelligenceService : ILeadIntelligenceService
{
    private readonly ILeadIntelligenceRepository _repository;
    private readonly IRawLeadService _rawLeadService;
    private readonly ILeadIntelligenceAnalyzer _analyzer;

    public LeadIntelligenceService(
        ILeadIntelligenceRepository repository,
        IRawLeadService rawLeadService,
        ILeadIntelligenceAnalyzer analyzer)
    {
        _repository = repository;
        _rawLeadService = rawLeadService;
        _analyzer = analyzer;
    }

    public Task<IEnumerable<LeadIntelligence>> GetAllActiveAsync() =>
        _repository.GetAllActiveAsync();

    public Task<LeadIntelligence?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<LeadIntelligence?> GetByContentHashAsync(string contentHash) =>
        _repository.GetByContentHashAsync(contentHash);

    public async Task CreateAsync(int leadId, string contentHash, double familiarityIndex, double dataDensityScore)
    {
        await EnsureActiveLeadAsync(leadId);

        var now = DateTime.UtcNow;
        await _repository.AddAsync(new LeadIntelligence
        {
            LeadId = leadId,
            ContentHash = contentHash,
            FamiliarityIndex = familiarityIndex,
            DataDensityScore = dataDensityScore,
            LastAnalyzedAt = now,
            ValidFrom = now
        });
    }

    public async Task<LeadIntelligence> AnalyzeLeadAsync(int leadId, CancellationToken cancellationToken = default)
    {
        var lead = await _rawLeadService.GetByIdAsync(leadId);
        if (lead is null || lead.ValidTo is not null)
        {
            throw new InvalidOperationException("Select an active raw lead before running analysis.");
        }

        var analysis = await _analyzer.AnalyzeAsync(lead.RawJsonData, cancellationToken);
        var contentHash = ComputeSha256(lead.RawJsonData);
        var now = DateTime.UtcNow;
        var existing = await _repository.GetByLeadIdAsync(leadId);

        if (existing is not null)
        {
            existing.ContentHash = contentHash;
            existing.FamiliarityIndex = analysis.FamiliarityIndex;
            existing.DataDensityScore = analysis.DataDensityScore;
            existing.LastAnalyzedAt = now;
            await _repository.UpdateAsync(existing);
            return existing;
        }

        var entity = new LeadIntelligence
        {
            LeadId = leadId,
            ContentHash = contentHash,
            FamiliarityIndex = analysis.FamiliarityIndex,
            DataDensityScore = analysis.DataDensityScore,
            LastAnalyzedAt = now,
            ValidFrom = now
        };

        await _repository.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(int id, int leadId, string contentHash, double familiarityIndex, double dataDensityScore)
    {
        await EnsureActiveLeadAsync(leadId);

        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return;

        entity.LeadId = leadId;
        entity.ContentHash = contentHash;
        entity.FamiliarityIndex = familiarityIndex;
        entity.DataDensityScore = dataDensityScore;
        entity.LastAnalyzedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
    }

    public Task SoftDeleteAsync(int id) =>
        _repository.SoftDeleteAsync(id);

    public string ComputeSha256(string content) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();

    private async Task EnsureActiveLeadAsync(int leadId)
    {
        var lead = await _rawLeadService.GetByIdAsync(leadId);
        if (lead is null || lead.ValidTo is not null)
        {
            throw new InvalidOperationException("Select an active raw lead before saving lead intelligence.");
        }
    }
}
