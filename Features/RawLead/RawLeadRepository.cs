using cesar.Data;
using Microsoft.EntityFrameworkCore;

namespace cesar.Features.RawLead;

using RawLead = global::cesar.Features.RawLead.Entities.RawLead;

public interface IRawLeadRepository
{
    Task<IEnumerable<RawLead>> GetAllActiveAsync();
    Task<IEnumerable<RawLead>> SearchActiveAsync(string term, int take = 10);
    Task<RawLead?> GetByIdAsync(int id);
    Task AddAsync(RawLead lead);
    Task AddRangeAsync(IEnumerable<RawLead> leads);
    Task UpdateAsync(RawLead lead);
    Task SoftDeleteAsync(int id);
}

public class RawLeadRepository : IRawLeadRepository
{
    private readonly AppDbContext _context;

    public RawLeadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RawLead>> GetAllActiveAsync() =>
        await _context.RawLeads
            .Where(r => r.ValidTo == null)
            .OrderByDescending(r => r.IngestedAt)
            .ToListAsync();

    public async Task<IEnumerable<RawLead>> SearchActiveAsync(string term, int take = 10)
    {
        var activeLeads = await _context.RawLeads
            .Where(r => r.ValidTo == null)
            .OrderByDescending(r => r.IngestedAt)
            .ToListAsync();

        if (string.IsNullOrWhiteSpace(term))
        {
            return activeLeads.Take(take);
        }

        return activeLeads
            .Where(r =>
                r.SourceSystem.Contains(term, StringComparison.OrdinalIgnoreCase)
                || r.ExternalId.Contains(term, StringComparison.OrdinalIgnoreCase)
                || r.RawJsonData.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Take(take);
    }

    public async Task<RawLead?> GetByIdAsync(int id) =>
        await _context.RawLeads.FindAsync(id);

    public async Task AddAsync(RawLead lead)
    {
        _context.RawLeads.Add(lead);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<RawLead> leads)
    {
        _context.RawLeads.AddRange(leads);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RawLead lead)
    {
        _context.RawLeads.Update(lead);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var lead = await _context.RawLeads.FindAsync(id);
        if (lead is null) return;

        lead.ValidTo = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
