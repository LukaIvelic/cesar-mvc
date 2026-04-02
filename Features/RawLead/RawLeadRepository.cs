using cesar.Data;
using Microsoft.EntityFrameworkCore;

namespace cesar.Features.RawLead;

using RawLead = global::cesar.Features.RawLead.Entities.RawLead;

public interface IRawLeadRepository
{
    Task<IEnumerable<RawLead>> GetAllActiveAsync();
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
        await _context.RawLeads.Where(r => r.ValidTo == null).ToListAsync();

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
