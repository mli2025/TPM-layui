namespace Infrastructure.Cache;

public interface ICacheContext
{
    void Set<T>(string key, T value, TimeSpan? expire = null);
    T? Get<T>(string key);
    bool Exists(string key);
    void Remove(string key);
}
