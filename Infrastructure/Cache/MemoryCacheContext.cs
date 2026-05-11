using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Cache;

public class MemoryCacheContext : ICacheContext
{
    private readonly IMemoryCache _cache;

    public MemoryCacheContext(IMemoryCache cache) => _cache = cache;

    public void Set<T>(string key, T value, TimeSpan? expire = null)
    {
        var opt = new MemoryCacheEntryOptions();
        if (expire.HasValue) opt.SetAbsoluteExpiration(expire.Value);
        else opt.SetSlidingExpiration(TimeSpan.FromHours(8));
        _cache.Set(key, value, opt);
    }

    public T? Get<T>(string key) => _cache.TryGetValue(key, out var v) ? (T?)v : default;

    public bool Exists(string key) => _cache.TryGetValue(key, out _);

    public void Remove(string key) => _cache.Remove(key);
}
