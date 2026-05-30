using System.Reflection;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.Web.Services;

/// <summary>
/// 审计追踪服务：通过反射对比实体新旧值，逐字段写入 Sys_AuditTrail。
/// 写入失败不影响主业务（吞异常 + 记日志）。
/// </summary>
public sealed class AuditService
{
    private readonly IRepository<Sys_AuditTrail> _repo;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<AuditService> _logger;

    // 不参与对比的字段
    private static readonly HashSet<string> IgnoreFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CreateDate", "UpdateDate", "CreateUserId", "UpdateUserId", "Password"
    };

    public AuditService(IRepository<Sys_AuditTrail> repo, IHttpContextAccessor http, ILogger<AuditService> logger)
    {
        _repo = repo;
        _http = http;
        _logger = logger;
    }

    /// <summary>记录新增（整体快照）</summary>
    public void WriteCreate<T>(string targetType, string targetId, T entity,
        long? userId = null, string? userAccount = null, string? module = null, string? reason = null)
    {
        try
        {
            _repo.Insert(BuildRow(targetType, targetId, "CREATE", null, "(新建)", userId, userAccount, module, reason));
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit WriteCreate failed: {T}/{Id}", targetType, targetId); }
    }

    /// <summary>记录删除</summary>
    public void WriteDelete(string targetType, string targetId,
        long? userId = null, string? userAccount = null, string? module = null, string? reason = null)
    {
        try
        {
            _repo.Insert(BuildRow(targetType, targetId, "DELETE", "(已删除)", null, userId, userAccount, module, reason));
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit WriteDelete failed: {T}/{Id}", targetType, targetId); }
    }

    /// <summary>对比新旧实体，逐变更字段写入；oldEntity 为 null 视为新增</summary>
    public int WriteDiff<T>(string targetType, string targetId, T? oldEntity, T newEntity,
        long? userId = null, string? userAccount = null, string? module = null, string? reason = null) where T : class
    {
        if (oldEntity == null)
        {
            WriteCreate(targetType, targetId, newEntity, userId, userAccount, module, reason);
            return 1;
        }
        var n = 0;
        try
        {
            foreach (var p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead || IgnoreFields.Contains(p.Name)) continue;
                if (!IsSimple(p.PropertyType)) continue;
                var ov = p.GetValue(oldEntity);
                var nv = p.GetValue(newEntity);
                if (Equals(ToStr(ov), ToStr(nv))) continue;
                _repo.Insert(BuildRow(targetType, targetId, "UPDATE", ToStr(ov), ToStr(nv), userId, userAccount, module, reason, p.Name));
                n++;
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit WriteDiff failed: {T}/{Id}", targetType, targetId); }
        return n;
    }

    private Sys_AuditTrail BuildRow(string targetType, string targetId, string action,
        string? oldVal, string? newVal, long? userId, string? userAccount, string? module, string? reason, string? field = null)
    {
        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        return new Sys_AuditTrail
        {
            TargetType = targetType,
            TargetId = targetId,
            ActionType = action,
            FieldName = field,
            OldValue = oldVal,
            NewValue = newVal,
            UserId = userId,
            UserAccount = userAccount,
            Module = module,
            Reason = reason,
            IpAddress = ip,
            CreateDate = DateTime.Now
        };
    }

    private static bool IsSimple(Type t)
    {
        var u = Nullable.GetUnderlyingType(t) ?? t;
        return u.IsSimpleType();
    }

    private static string? ToStr(object? v)
    {
        if (v == null) return null;
        if (v is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss");
        if (v is bool b) return b ? "true" : "false";
        if (v is decimal m) return m.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return v.ToString();
    }
}

internal static class AuditTypeExtensions
{
    public static bool IsSimpleType(this Type t)
        => t.IsPrimitive || t.IsEnum
           || t == typeof(string) || t == typeof(decimal)
           || t == typeof(DateTime) || t == typeof(Guid)
           || t == typeof(TimeSpan);
}
