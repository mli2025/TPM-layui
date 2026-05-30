using System.Text.RegularExpressions;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

/// <summary>
/// 自定义报表引擎：报表定义 CRUD + 受限 SELECT 执行。
/// QueryDef 存放一条只读 SELECT 语句（管理员定义），执行前做白名单校验。
/// </summary>
public class ReportApp : BaseApp<Sys_ReportDef>
{
    private static readonly string[] Forbidden =
    {
        "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "CREATE", "TRUNCATE",
        "EXEC", "EXECUTE", "MERGE", "GRANT", "REVOKE", "BACKUP", "RESTORE", "SHUTDOWN", "XP_", "SP_"
    };

    public ReportApp(IUnitWork unitWork, IRepository<Sys_ReportDef> repository) : base(unitWork, repository) { }

    public List<Sys_ReportDef> ListAll()
        => Repository.Find(null, null, "[Module],[Id] DESC").ToList();

    public long Save(Sys_ReportDef m)
    {
        m.Code = (m.Code ?? string.Empty).Trim();
        m.Name = (m.Name ?? string.Empty).Trim();
        if (m.Id == 0) { m.CreateDate = DateTime.Now; Repository.Insert(m); }
        else Repository.Update(m);
        return m.Id;
    }

    /// <summary>校验 SQL 是否为安全只读 SELECT</summary>
    public static (bool ok, string? err) ValidateSql(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql)) return (false, "SQL 为空");
        var s = sql.Trim();
        if (s.EndsWith(";")) s = s[..^1].Trim();
        if (s.Contains(';')) return (false, "不允许多语句");
        var upper = s.ToUpperInvariant();
        if (!upper.StartsWith("SELECT") && !upper.StartsWith("WITH"))
            return (false, "仅允许 SELECT/WITH 查询");
        foreach (var kw in Forbidden)
            if (Regex.IsMatch(upper, $@"\b{kw}\b") || upper.Contains(kw))
                return (false, $"包含禁止关键字: {kw}");
        return (true, null);
    }

    /// <summary>执行报表定义，返回动态行集合</summary>
    public (bool ok, string? err, IEnumerable<dynamic> rows) Run(long id)
    {
        var def = Get(id);
        if (def == null) return (false, "报表不存在", Array.Empty<dynamic>());
        var (ok, err) = ValidateSql(def.QueryDef);
        if (!ok) return (false, err, Array.Empty<dynamic>());
        try
        {
            var rows = Repository.Query<dynamic>(def.QueryDef!);
            return (true, null, rows);
        }
        catch (Exception ex)
        {
            return (false, "执行失败: " + ex.Message, Array.Empty<dynamic>());
        }
    }
}
