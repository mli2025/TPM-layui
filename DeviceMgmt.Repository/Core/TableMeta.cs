using System.Collections.Concurrent;
using System.Reflection;

namespace DeviceMgmt.Repository.Core;

internal static class TableMeta
{
    private static readonly ConcurrentDictionary<Type, (string Table, PropertyInfo[] Props, PropertyInfo? Id)> Cache = new();

    public static (string Table, PropertyInfo[] Props, PropertyInfo? Id) Get(Type type)
    {
        return Cache.GetOrAdd(type, t =>
        {
            var attr = t.GetCustomAttribute<TableAttribute>();
            var table = attr != null ? $"[{attr.Schema}].[{attr.Name}]" : $"[dbo].[{t.Name}]";
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .ToArray();
            var id = props.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            return (table, props, id);
        });
    }
}
