using DeviceMgmt.App.Apps.Basic;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Basic;

/// <summary>员工主数据：系统管理 → 基础资料 → 员工主数据</summary>
public class Basic_EmployeeController : BaseController
{
    private readonly EmployeeApp _app;
    private readonly IRepository<Basic_Employee> _repo;
    private readonly IRepository<Sys_Dept> _deptRepo;

    public Basic_EmployeeController(IAuth auth,
        EmployeeApp app,
        IRepository<Basic_Employee> repo,
        IRepository<Sys_Dept> deptRepo) : base(auth)
    {
        _app = app;
        _repo = repo;
        _deptRepo = deptRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req)
    {
        var data = _app.Getmainlist(req);
        // 把 DeptId -> DeptName 注入，前端列直接展示
        if (data?.data is IEnumerable<Basic_Employee> rows)
        {
            var deptMap = _deptRepo.Find(null, null, "[Id] ASC").ToDictionary(d => d.Id, d => d.DeptName);
            var enriched = rows.Select(e => new
            {
                e.Id,
                e.EmployeeNumber,
                e.Name,
                e.DeptId,
                DeptName = deptMap.TryGetValue(e.DeptId, out var dn) ? dn : "",
                e.Status
            }).ToList();
            return Json(new TableData { code = 0, count = data.count, data = enriched });
        }
        return Json(data);
    }

    /// <summary>下拉用：所有启用部门</summary>
    [HttpGet]
    public IActionResult GetDepts()
    {
        var rows = _deptRepo.Find("[Status]=1", null, "[ParentId] ASC, [Id] ASC")
            .Select(d => new { Id = d.Id.ToString(), d.DeptName, d.ParentId })
            .ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpPost]
    public IActionResult Save([FromForm] Basic_Employee model)
    {
        try
        {
            var id = _app.Save(model);
            return Json(new ResponseData { code = 0, data = id, msg = "ok" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new ResponseData { code = 400, msg = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "参数错误" });
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult BatchDelete([FromForm] long[] ids)
    {
        if (ids == null || ids.Length == 0) return Json(new ResponseData { code = 400, msg = "请选择员工" });
        _app.Delete(ids);
        return Json(new ResponseData { code = 0, msg = $"已删除 {ids.Length} 条" });
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Basic_Employee>) ?? Enumerable.Empty<Basic_Employee>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Basic_Employee");
        return File(bytes, "application/vnd.ms-excel", "Basic_Employee.xls");
    }
}
