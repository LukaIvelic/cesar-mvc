using cesar.Data;
using Microsoft.EntityFrameworkCore;

namespace cesar.Features.LeadIntelligence;

using LeadIntelligence = global::cesar.Features.LeadIntelligence.Entities.LeadIntelligence;

public interface ILeadIntelligenceRepository
{
    Task<IEnumerable<LeadIntelligence>> GetAllActiveAsync();
    Task<LeadIntelligence?> GetByIdAsync(int id);
    Task<LeadIntelligence?> GetByLeadIdAsync(int leadId);
    Task<LeadIntelligence?> GetByContentHashAsync(string contentHash);
    Task AddAsync(LeadIntelligence entity);
    Task UpdateAsync(LeadIntelligence entity);
    Task SoftDeleteAsync(int id);
}

public class LeadIntelligenceRepository : ILeadIntelligenceRepository
{
    private readonly AppDbContext _context;

    public LeadIntelligenceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeadIntelligence>> GetAllActiveAsync() =>
        await _context.LeadIntelligences.Where(l => l.ValidTo == null).ToListAsync();

    public async Task<LeadIntelligence?> GetByIdAsync(int id) =>
        await _context.LeadIntelligences.FindAsync(id);

    public async Task<LeadIntelligence?> GetByLeadIdAsync(int leadId) =>
        await _context.LeadIntelligences
            .Where(l => l.ValidTo == null && l.LeadId == leadId)
            .OrderByDescending(l => l.LastAnalyzedAt)
            .FirstOrDefaultAsync();

    public async Task<LeadIntelligence?> GetByContentHashAsync(string contentHash) =>
        await _context.LeadIntelligences
            .Where(l => l.ValidTo == null && l.ContentHash == contentHash)
            .FirstOrDefaultAsync();

    public async Task AddAsync(LeadIntelligence entity)
    {
        _context.LeadIntelligences.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LeadIntelligence entity)
    {
        _context.LeadIntelligences.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.LeadIntelligences.FindAsync(id);
        if (entity is null) return;

        entity.ValidTo = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
