using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Core;
using DeviceMgmt.Repository.Interface;
using Infrastructure;

namespace DeviceMgmt.App.Apps;

public class BaseApp<T> where T : Entity
{
    protected readonly IUnitWork UnitWork;
    protected readonly IRepository<T> Repository;

    public BaseApp(IUnitWork unitWork, IRepository<T> repository)
    {
        UnitWork = unitWork;
        Repository = repository;
    }

    public virtual T? Get(long id) => Repository.FindSingle(id);

    public virtual void Delete(long id) => Repository.Delete(id);

    public virtual void Delete(long[] ids) => Repository.Delete(ids);

    public virtual long Add(T entity) => Repository.Insert(entity);

    public virtual int Update(T entity) => Repository.Update(entity);

    public virtual TableData Getmainlist(PageReq req, long? deptId = null)
    {
        var filters = GetSearchCondition(req.searchParam);
        var orderBy = BuildOrderBy(req.sfield, req.sorder);
        var (data, total) = Repository.FindPaged(filters, req.page, req.limit, orderBy);
        return new TableData { code = 200, count = total, data = data };
    }

    protected static string BuildOrderBy(string? sfield, string? sorder)
    {
        if (string.IsNullOrWhiteSpace(sfield)) return "[Id] DESC";
        var dir = string.IsNullOrWhiteSpace(sorder) ? "ASC" : sorder.ToUpperInvariant();
        if (dir != "ASC" && dir != "DESC") dir = "ASC";
        var safe = new string(sfield.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        if (string.IsNullOrEmpty(safe)) return "[Id] DESC";
        return $"[{safe}] {dir}";
    }

    protected Filter[] GetSearchCondition(List<searchParam>? searchParam)
    {
        var list = new List<Filter>();
        if (searchParam == null) return list.ToArray();
        foreach (var item in searchParam)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.value)) continue;
            list.Add(new Filter
            {
                field = item.field,
                Value = item.value,
                conditional = string.IsNullOrEmpty(item.conditional) ? "like" : item.conditional
            });
        }
        return list.ToArray();
    }

    protected string GetSearchSQL(List<searchParam>? searchParam)
    {
        if (searchParam == null) return string.Empty;
        var filter = string.Empty;
        foreach (var item in searchParam)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.value)) continue;
            var cond = string.IsNullOrEmpty(item.conditional) ? "like" : item.conditional;
            if (cond.Equals("like", StringComparison.OrdinalIgnoreCase))
                filter += $" AND [{item.field}] LIKE '%{item.value}%'";
            else
                filter += $" AND [{item.field}] {cond} '{item.value}'";
        }
        return filter;
    }
}
