using cesar.Data;
using cesar.Features.JsonKeyStats.Entities;
using Microsoft.EntityFrameworkCore;

namespace cesar.Features.JsonKeyStats;

public interface IJsonKeyStatRepository
{
    Task<IEnumerable<JsonKeyStat>> GetAllActiveAsync();
    Task<JsonKeyStat?> GetByIdAsync(int id);
    Task<JsonKeyStat?> GetByKeyAsync(string key);
    Task AddAsync(JsonKeyStat entity);
    Task UpdateAsync(JsonKeyStat entity);
    Task SoftDeleteAsync(int id);
}

public class JsonKeyStatRepository : IJsonKeyStatRepository
{
    private readonly AppDbContext _context;

    public JsonKeyStatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JsonKeyStat>> GetAllActiveAsync() =>
        await _context.JsonKeyStats.Where(k => k.ValidTo == null).OrderByDescending(k => k.Occurrences).ToListAsync();

    public async Task<JsonKeyStat?> GetByIdAsync(int id) =>
        await _context.JsonKeyStats.FindAsync(id);

    public async Task<JsonKeyStat?> GetByKeyAsync(string key) =>
        await _context.JsonKeyStats.Where(k => k.ValidTo == null && k.Key == key).FirstOrDefaultAsync();

    public async Task AddAsync(JsonKeyStat entity)
    {
        _context.JsonKeyStats.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(JsonKeyStat entity)
    {
        _context.JsonKeyStats.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.JsonKeyStats.FindAsync(id);
        if (entity is null) return;

        entity.ValidTo = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
