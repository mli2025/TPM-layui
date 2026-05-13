using System.Data;
using System.Text;
using Dapper;
using DeviceMgmt.Repository.Interface;
using Infrastructure;

namespace DeviceMgmt.Repository.Core;

public class Repository<T> : IRepository<T> where T : Entity
{
    private readonly IUnitWork _unitWork;

    public Repository(IUnitWork unitWork) => _unitWork = unitWork;

    private static (string Table, System.Reflection.PropertyInfo[] Props, System.Reflection.PropertyInfo? Id) Meta()
        => TableMeta.Get(typeof(T));

    public T? FindSingle(long id)
    {
        var (table, _, _) = Meta();
        using var conn = _unitWork.OpenConnection();
        return conn.QueryFirstOrDefault<T>($"SELECT TOP 1 * FROM {table} WHERE [Id]=@Id", new { Id = id });
    }

    public T? FindSingle(string whereSql, object? param = null)
    {
        var (table, _, _) = Meta();
        var sql = $"SELECT TOP 1 * FROM {table}";
        if (!string.IsNullOrWhiteSpace(whereSql)) sql += " WHERE " + whereSql;
        using var conn = _unitWork.OpenConnection();
        return conn.QueryFirstOrDefault<T>(sql, param);
    }

    public IEnumerable<T> Find(string? whereSql = null, object? param = null, string? orderBy = null)
    {
        var (table, _, _) = Meta();
        var sql = $"SELECT * FROM {table}";
        if (!string.IsNullOrWhiteSpace(whereSql)) sql += " WHERE " + whereSql;
        if (!string.IsNullOrWhiteSpace(orderBy)) sql += " ORDER BY " + orderBy;
        using var conn = _unitWork.OpenConnection();
        return conn.Query<T>(sql, param).ToList();
    }

    public (IEnumerable<T> data, int total) FindPaged(Filter[] filters, int page, int limit, string? orderBy = null)
    {
        var (table, _, _) = Meta();
        var (where, param) = BuildWhere(filters);
        var totalSql = $"SELECT COUNT(1) FROM {table}" + (string.IsNullOrEmpty(where) ? string.Empty : " WHERE " + where);
        if (string.IsNullOrWhiteSpace(orderBy)) orderBy = "[Id] DESC";
        var pageSql = $"SELECT * FROM {table}"
                       + (string.IsNullOrEmpty(where) ? string.Empty : " WHERE " + where)
                       + $" ORDER BY {orderBy} OFFSET @__skip ROWS FETCH NEXT @__take ROWS ONLY";
        page = Math.Max(1, page);
        limit = Math.Max(1, limit);
        param["__skip"] = (page - 1) * limit;
        param["__take"] = limit;
        using var conn = _unitWork.OpenConnection();
        var total = conn.ExecuteScalar<int>(totalSql, param);
        var data = conn.Query<T>(pageSql, param).ToList();
        return (data, total);
    }

    public long Insert(T entity)
    {
        // [Id] is bigint IDENTITY(1,1) in all tables; never include it in INSERT.
        // Return new Id from SCOPE_IDENTITY() in a single round-trip.
        var (table, props, _) = Meta();
        var insertProps = props.Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)).ToArray();
        var cols = string.Join(",", insertProps.Select(p => $"[{p.Name}]"));
        var vals = string.Join(",", insertProps.Select(p => "@" + p.Name));
        var sql = $"INSERT INTO {table}({cols}) VALUES({vals}); SELECT CAST(SCOPE_IDENTITY() AS bigint);";
        using var conn = _unitWork.OpenConnection();
        var newId = conn.ExecuteScalar<long>(sql, entity);
        entity.Id = newId;
        return newId;
    }

    public int Update(T entity)
    {
        var (table, props, _) = Meta();
        var setProps = props.Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)).ToArray();
        var sets = string.Join(",", setProps.Select(p => $"[{p.Name}]=@{p.Name}"));
        var sql = $"UPDATE {table} SET {sets} WHERE [Id]=@Id";
        using var conn = _unitWork.OpenConnection();
        return conn.Execute(sql, entity);
    }

    public int Delete(long id)
    {
        var (table, _, _) = Meta();
        using var conn = _unitWork.OpenConnection();
        return conn.Execute($"DELETE FROM {table} WHERE [Id]=@Id", new { Id = id });
    }

    public int Delete(IEnumerable<long> ids)
    {
        var (table, _, _) = Meta();
        using var conn = _unitWork.OpenConnection();
        return conn.Execute($"DELETE FROM {table} WHERE [Id] IN @Ids", new { Ids = ids });
    }

    public int ExecuteSql(string sql, object? param = null)
    {
        using var conn = _unitWork.OpenConnection();
        return conn.Execute(sql, param);
    }

    public IEnumerable<TResult> Query<TResult>(string sql, object? param = null)
    {
        using var conn = _unitWork.OpenConnection();
        return conn.Query<TResult>(sql, param).ToList();
    }

    public int Count(string? whereSql = null, object? param = null)
    {
        var (table, _, _) = Meta();
        var sql = $"SELECT COUNT(1) FROM {table}";
        if (!string.IsNullOrWhiteSpace(whereSql)) sql += " WHERE " + whereSql;
        using var conn = _unitWork.OpenConnection();
        return conn.ExecuteScalar<int>(sql, param);
    }

    private static (string Where, Dictionary<string, object?> Param) BuildWhere(Filter[] filters)
    {
        var param = new Dictionary<string, object?>();
        if (filters == null || filters.Length == 0) return (string.Empty, param);
        var sb = new StringBuilder();
        for (var i = 0; i < filters.Length; i++)
        {
            var f = filters[i];
            if (string.IsNullOrWhiteSpace(f.field) || f.Value == null) continue;
            if (sb.Length > 0) sb.Append(" AND ");
            var p = $"@p{i}";
            var cond = string.IsNullOrEmpty(f.conditional) ? "like" : f.conditional;
            switch (cond.ToLowerInvariant())
            {
                case "like":
                    sb.Append($"[{f.field}] LIKE {p}");
                    param[$"p{i}"] = "%" + f.Value + "%";
                    break;
                case "=":
                case "==":
                case "eq":
                    sb.Append($"[{f.field}]={p}");
                    param[$"p{i}"] = f.Value;
                    break;
                case "<>":
                case "!=":
                case "ne":
                    sb.Append($"[{f.field}]<>{p}");
                    param[$"p{i}"] = f.Value;
                    break;
                case ">":
                case "gt":
                    sb.Append($"[{f.field}]>{p}");
                    param[$"p{i}"] = f.Value;
                    break;
                case ">=":
                case "ge":
                    sb.Append($"[{f.field}]>={p}");
                    param[$"p{i}"] = f.Value;
                    break;
                case "<":
                case "lt":
                    sb.Append($"[{f.field}]<{p}");
                    param[$"p{i}"] = f.Value;
                    break;
                case "<=":
                case "le":
                    sb.Append($"[{f.field}]<={p}");
                    param[$"p{i}"] = f.Value;
                    break;
                default:
                    sb.Append($"[{f.field}] LIKE {p}");
                    param[$"p{i}"] = "%" + f.Value + "%";
                    break;
            }
        }
        return (sb.ToString(), param);
    }
}
