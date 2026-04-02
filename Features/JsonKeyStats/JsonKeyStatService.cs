using cesar.Features.JsonKeyStats.Entities;

namespace cesar.Features.JsonKeyStats;

public interface IJsonKeyStatService
{
    Task<IEnumerable<JsonKeyStat>> GetAllActiveAsync();
    Task<JsonKeyStat?> GetByIdAsync(int id);
    Task<JsonKeyStat?> GetByKeyAsync(string key);
    Task CreateAsync(string key, int occurrences = 1);
    Task IncrementAsync(string key);
    Task TrackKeysAsync(IEnumerable<string> keys);
    Task UpdateAsync(int id, string key, int occurrences);
    Task SoftDeleteAsync(int id);
}

public class JsonKeyStatService : IJsonKeyStatService
{
    private readonly IJsonKeyStatRepository _repository;

    public JsonKeyStatService(IJsonKeyStatRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<JsonKeyStat>> GetAllActiveAsync() =>
        _repository.GetAllActiveAsync();

    public Task<JsonKeyStat?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<JsonKeyStat?> GetByKeyAsync(string key) =>
        _repository.GetByKeyAsync(key);

    public async Task CreateAsync(string key, int occurrences = 1)
    {
        await _repository.AddAsync(new JsonKeyStat
        {
            Key = key,
            Occurrences = occurrences,
            ValidFrom = DateTime.UtcNow
        });
    }

    public async Task IncrementAsync(string key)
    {
        var existing = await _repository.GetByKeyAsync(key);
        if (existing is not null)
        {
            existing.Occurrences++;
            await _repository.UpdateAsync(existing);
        }
        else
        {
            await CreateAsync(key, 1);
        }
    }

    public async Task TrackKeysAsync(IEnumerable<string> keys)
    {
        foreach (var key in keys)
            await IncrementAsync(key);
    }

    public async Task UpdateAsync(int id, string key, int occurrences)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return;

        entity.Key = key;
        entity.Occurrences = occurrences;
        await _repository.UpdateAsync(entity);
    }

    public Task SoftDeleteAsync(int id) =>
        _repository.SoftDeleteAsync(id);
}
