using System.Security.Cryptography;
using System.Text;

namespace cesar.Features.LeadIntelligence;

using LeadIntelligence = global::cesar.Features.LeadIntelligence.Entities.LeadIntelligence;

public interface ILeadIntelligenceService
{
    Task<IEnumerable<LeadIntelligence>> GetAllActiveAsync();
    Task<LeadIntelligence?> GetByIdAsync(int id);
    Task<LeadIntelligence?> GetByContentHashAsync(string contentHash);
    Task CreateAsync(int leadId, string contentHash, double familiarityIndex, double dataDensityScore);
    Task UpdateAsync(int id, int leadId, string contentHash, double familiarityIndex, double dataDensityScore);
    Task SoftDeleteAsync(int id);
    string ComputeSha256(string content);
}

public class LeadIntelligenceService : ILeadIntelligenceService
{
    private readonly ILeadIntelligenceRepository _repository;

    public LeadIntelligenceService(ILeadIntelligenceRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<LeadIntelligence>> GetAllActiveAsync() =>
        _repository.GetAllActiveAsync();

    public Task<LeadIntelligence?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<LeadIntelligence?> GetByContentHashAsync(string contentHash) =>
        _repository.GetByContentHashAsync(contentHash);

    public async Task CreateAsync(int leadId, string contentHash, double familiarityIndex, double dataDensityScore)
    {
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

    public async Task UpdateAsync(int id, int leadId, string contentHash, double familiarityIndex, double dataDensityScore)
    {
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
}
