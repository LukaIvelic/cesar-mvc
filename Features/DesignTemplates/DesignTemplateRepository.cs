using cesar.Data;
using cesar.Features.DesignTemplates.Entities;
using Microsoft.EntityFrameworkCore;

namespace cesar.Features.DesignTemplates;

public interface IDesignTemplateRepository
{
    Task<IEnumerable<DesignTemplate>> GetAllActiveAsync();
    Task<DesignTemplate?> GetByIdAsync(int id);
    Task AddAsync(DesignTemplate template);
    Task UpdateAsync(DesignTemplate template);
    Task SoftDeleteAsync(int id);
}

public class DesignTemplateRepository : IDesignTemplateRepository
{
    private readonly AppDbContext _context;

    public DesignTemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DesignTemplate>> GetAllActiveAsync() =>
        await _context.DesignTemplates
            .Where(t => t.ValidTo == null)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

    public async Task<DesignTemplate?> GetByIdAsync(int id) =>
        await _context.DesignTemplates.FindAsync(id);

    public async Task AddAsync(DesignTemplate template)
    {
        _context.DesignTemplates.Add(template);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DesignTemplate template)
    {
        _context.DesignTemplates.Update(template);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var template = await _context.DesignTemplates.FindAsync(id);
        if (template is null) return;

        template.ValidTo = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
