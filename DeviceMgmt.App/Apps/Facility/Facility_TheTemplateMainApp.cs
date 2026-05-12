using System.Reflection;
using System.Text;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Core;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Infrastructure;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_TheTemplateMainApp : BaseApp<Facility_TheTemplateMain>
{
    public Facility_TheTemplateMainApp(IUnitWork unitWork, IRepository<Facility_TheTemplateMain> repository) : base(unitWork, repository)
    {
    }

    /// <summary>
    /// 模板列表：支持 <see cref="PageReq.key"/> 对模板编号、名称做 OR 模糊查询；避免仅按名称过滤且与 Layui where 合并导致条件残留。
    /// </summary>
    public TableData GetTemplateMainList(PageReq req)
    {
        var tableAttr = typeof(Facility_TheTemplateMain).GetCustomAttribute<TableAttribute>();
        var table = tableAttr != null ? $"[{tableAttr.Schema}].[{tableAttr.Name}]" : "[dbo].[Facility_TheTemplateMain]";

        var filters = GetSearchCondition(req.searchParam);
        var (whereSql, param) = BuildWhereFromFilters(filters);
        var kw = req.key?.Trim();
        if (!string.IsNullOrEmpty(kw))
        {
            if (!string.IsNullOrEmpty(whereSql)) whereSql += " AND ";
            whereSql += "([HNumber] LIKE @__kw OR [HName] LIKE @__kw)";
            param["__kw"] = "%" + kw + "%";
        }

        var orderBy = BuildOrderBy(req.sfield, req.sorder);
        var page = Math.Max(1, req.page);
        var limit = Math.Max(1, req.limit);

        var total = Repository.Count(whereSql, param);
        param["__skip"] = (page - 1) * limit;
        param["__take"] = limit;

        var pageSql = $"SELECT * FROM {table}"
                      + (string.IsNullOrEmpty(whereSql) ? string.Empty : " WHERE " + whereSql)
                      + $" ORDER BY {orderBy} OFFSET @__skip ROWS FETCH NEXT @__take ROWS ONLY";

        var rows = Repository.Query<Facility_TheTemplateMain>(pageSql, param).ToList();
        return new TableData { code = 0, count = total, data = rows };
    }

    private static (string Where, Dictionary<string, object?> Param) BuildWhereFromFilters(Filter[]? filters)
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
