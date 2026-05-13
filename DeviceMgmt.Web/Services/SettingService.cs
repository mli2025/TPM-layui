using System.Collections.Concurrent;
using System.Globalization;
using DeviceMgmt.Repository.Core;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceMgmt.Web.Services;

/// <summary>
/// 全局设置：启动时一次性加载到 ConcurrentDictionary，写入时同步落库 + 缓存
/// </summary>
public sealed class SettingService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, Sys_Setting> _cache = new(StringComparer.OrdinalIgnoreCase);
    private DateTime _lastLoad = DateTime.MinValue;

    public SettingService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public DateTime LastLoad => _lastLoad;

    /// <summary>重新从数据库装载全部设置</summary>
    public void Reload()
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Sys_Setting>>();
        var rows = repo.Find(null, null, "[Group],[Sort]");
        _cache.Clear();
        foreach (var r in rows)
        {
            _cache[r.Key] = r;
        }
        _lastLoad = DateTime.Now;
    }

    public IEnumerable<Sys_Setting> GetAll() => _cache.Values
        .OrderBy(x => x.Group).ThenBy(x => x.Sort);

    public Sys_Setting? GetEntity(string key)
        => _cache.TryGetValue(key, out var v) ? v : null;

    public string? GetString(string key, string? fallback = null)
        => _cache.TryGetValue(key, out var v) ? v.Value : fallback;

    public int GetInt(string key, int fallback = 0)
    {
        if (_cache.TryGetValue(key, out var v) && int.TryParse(v.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            return r;
        return fallback;
    }

    public long GetLong(string key, long fallback = 0)
    {
        if (_cache.TryGetValue(key, out var v) && long.TryParse(v.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            return r;
        return fallback;
    }

    public bool GetBool(string key, bool fallback = false)
    {
        if (_cache.TryGetValue(key, out var v) && bool.TryParse(v.Value, out var r))
            return r;
        return fallback;
    }

    /// <summary>批量更新设置（白名单：必须 Sys_Setting.Editable=true）</summary>
    public int UpdateMany(IDictionary<string, string?> values)
    {
        if (values == null || values.Count == 0) return 0;
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Sys_Setting>>();
        var count = 0;
        foreach (var kv in values)
        {
            if (!_cache.TryGetValue(kv.Key, out var s)) continue;
            if (!s.Editable) continue;
            repo.ExecuteSql(
                "UPDATE [Sys_Setting] SET [Value]=@v, [UpdateDate]=GETDATE() WHERE [Key]=@k",
                new { v = kv.Value, k = kv.Key });
            s.Value = kv.Value;
            s.UpdateDate = DateTime.Now;
            count++;
        }
        return count;
    }

    /// <summary>给前端注入的对象（脱敏后）：键值对扁平结构</summary>
    public Dictionary<string, object?> ToClientPayload()
    {
        var dict = new Dictionary<string, object?>();
        foreach (var s in _cache.Values)
        {
            object? boxed = s.ValueType switch
            {
                "int" or "long" => long.TryParse(s.Value, out var i) ? i : 0L,
                "bool" => bool.TryParse(s.Value, out var b) && b,
                _ => s.Value
            };
            dict[s.Key] = boxed;
        }
        return dict;
    }
}
