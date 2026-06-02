using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Basic;

/// <summary>生产资源（工作中心资源）查询，供设备台账选择器使用</summary>
public class Basic_ResourceController : BaseController
{
    private readonly IRepository<Basic_Resource> _repo;

    public Basic_ResourceController(IAuth auth, IRepository<Basic_Resource> repo) : base(auth) => _repo = repo;

    [HttpPost]
    public IActionResult GetPickerList()
    {
        var page = int.TryParse(Request.Form["page"], out var pg) && pg > 0 ? pg : 1;
        var limit = int.TryParse(Request.Form["limit"], out var lim) && lim > 0 ? lim : 20;
        var q = Request.Form["query"].ToString();
        if (string.IsNullOrWhiteSpace(q)) q = Request.Form["key"].ToString();
        var idEq = Request.Form["Id"].ToString();

        var where = "[Status]=1";
        object param;
        if (!string.IsNullOrWhiteSpace(idEq) && long.TryParse(idEq, out var rid))
        {
            where += " AND [Id]=@id";
            param = new { id = rid, __skip = (page - 1) * limit, __take = limit };
        }
        else if (!string.IsNullOrWhiteSpace(q))
        {
            where += " AND ([Code] LIKE @q OR [Name] LIKE @q)";
            param = new { q = "%" + q.Trim() + "%", __skip = (page - 1) * limit, __take = limit };
        }
        else
        {
            param = new { __skip = (page - 1) * limit, __take = limit };
        }

        try
        {
            var total = _repo.Count(where, param);
            var rows = _repo.Query<Basic_Resource>(
                $"SELECT * FROM [Basic_Resource] WHERE {where} ORDER BY [Code] ASC OFFSET @__skip ROWS FETCH NEXT @__take ROWS ONLY",
                param).ToList();
            return Json(new TableData { code = 0, count = total, data = rows });
        }
        catch
        {
            return Json(new TableData { code = 0, count = 0, data = Array.Empty<Basic_Resource>(), msg = "生产资源表不可用" });
        }
    }
}
