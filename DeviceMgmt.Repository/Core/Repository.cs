using System.Data;
using System.Text;
using Dapper;
using DeviceMgmt.Repository.Interface;
using Infrastructure;

namespace DeviceMgmt.Repository.Core;

public class Repository<T> : IRepository<T> where T : Entity
{
    private readonly IUnitWork _unitWork;
    private readonly IAuditContext? _audit;

    public Repository(IUnitWork unitWork, IAuditContext? audit = null)
    {
        _unitWork = unitWork;
        _audit = audit;
    }

    private static (string Table, System.Reflection.PropertyInfo[] Props, System.Reflection.PropertyInfo? Id) Meta()
        => TableMeta.Get(typeof(T));

    // 审计自身相关表不参与审计，避免无限递归
    private static readonly HashSet<string> AuditExcludedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Sys_AuditTrail", "Sys_OperationLog", "Sys_LoginLog"
    };

    // 簿记/敏感字段不记入字段级审计
    private static readonly HashSet<string> AuditIgnoreFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CreateDate", "UpdateDate", "CreateUserId", "UpdateUserId", "Password",
        "FGC_Creator", "FGC_CreateDate", "FGC_LastModifier", "FGC_LastModifyDate"
    };

    private static bool AuditEnabled => !AuditExcludedTypes.Contains(typeof(T).Name);

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
        long newId;
        using (var conn = _unitWork.OpenConnection())
            newId = conn.ExecuteScalar<long>(sql, entity);
        entity.Id = newId;
        TryAuditRow("CREATE", newId.ToString(), null, null, "(新建)");
        return newId;
    }

    public int Update(T entity)
    {
        var (table, props, _) = Meta();
        // 更新前取旧值以做字段级对比（URS 306：旧值/新值）
        var old = AuditEnabled ? FindSingle(entity.Id) : null;
        var setProps = props.Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)).ToArray();
        var sets = string.Join(",", setProps.Select(p => $"[{p.Name}]=@{p.Name}"));
        var sql = $"UPDATE {table} SET {sets} WHERE [Id]=@Id";
        int rows;
        using (var conn = _unitWork.OpenConnection())
            rows = conn.Execute(sql, entity);
        if (rows > 0 && old != null) TryAuditDiff(entity.Id, old, entity);
        return rows;
    }

    public int Delete(long id)
    {
        var (table, _, _) = Meta();
        int rows;
        using (var conn = _unitWork.OpenConnection())
            rows = conn.Execute($"DELETE FROM {table} WHERE [Id]=@Id", new { Id = id });
        if (rows > 0) TryAuditRow("DELETE", id.ToString(), null, "(已删除)", null);
        return rows;
    }

    public int Delete(IEnumerable<long> ids)
    {
        var (table, _, _) = Meta();
        var idList = ids?.ToList() ?? new List<long>();
        int rows;
        using (var conn = _unitWork.OpenConnection())
            rows = conn.Execute($"DELETE FROM {table} WHERE [Id] IN @Ids", new { Ids = idList });
        if (rows > 0 && AuditEnabled)
            foreach (var id in idList) TryAuditRow("DELETE", id.ToString(), null, "(已删除)", null);
        return rows;
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

    // ------------------------------------------------------------------
    // 字段级审计（全局、自动、不可关闭）。任何写入失败都不得影响主业务。
    // ------------------------------------------------------------------
    private void TryAuditDiff(long id, T oldEntity, T newEntity)
    {
        if (!AuditEnabled) return;
        try
        {
            foreach (var p in typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!p.CanRead || AuditIgnoreFields.Contains(p.Name)) continue;
                var u = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                if (!IsSimple(u)) continue;
                var os = ToStr(p.GetValue(oldEntity));
                var ns = ToStr(p.GetValue(newEntity));
                if (string.Equals(os, ns)) continue;
                InsertAuditRow("UPDATE", id.ToString(), p.Name, os, ns);
            }
        }
        catch { /* 审计失败静默，不影响业务 */ }
    }

    private void TryAuditRow(string action, string targetId, string? field, string? oldVal, string? newVal)
    {
        if (!AuditEnabled) return;
        try { InsertAuditRow(action, targetId, field, oldVal, newVal); }
        catch { /* 审计失败静默，不影响业务 */ }
    }

    private void InsertAuditRow(string action, string targetId, string? field, string? oldVal, string? newVal)
    {
        const string sql =
            "INSERT INTO [Sys_AuditTrail]([UserId],[UserAccount],[Module],[TargetType],[TargetId],[ActionType],[FieldName],[OldValue],[NewValue],[Reason],[IpAddress],[CreateDate]) " +
            "VALUES(@UserId,@UserAccount,@Module,@TargetType,@TargetId,@ActionType,@FieldName,@OldValue,@NewValue,@Reason,@IpAddress,@CreateDate)";
        using var conn = _unitWork.OpenConnection();
        conn.Execute(sql, new
        {
            UserId = _audit?.UserId,
            UserAccount = _audit?.UserAccount,
            Module = _audit?.Module,
            TargetType = typeof(T).Name,
            TargetId = targetId,
            ActionType = action,
            FieldName = field,
            OldValue = oldVal,
            NewValue = newVal,
            Reason = _audit?.Reason,
            IpAddress = _audit?.IpAddress,
            CreateDate = DateTime.Now
        });
    }

    private static bool IsSimple(Type t)
        => t.IsPrimitive || t.IsEnum
           || t == typeof(string) || t == typeof(decimal)
           || t == typeof(DateTime) || t == typeof(Guid) || t == typeof(TimeSpan);

    private static string? ToStr(object? v)
    {
        if (v == null) return null;
        if (v is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");
        if (v is bool b) return b ? "true" : "false";
        if (v is decimal m) return m.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return v.ToString();
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
