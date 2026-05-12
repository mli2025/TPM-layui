using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

public class Sys_DeptController : BaseController
{
    private readonly DeptApp _app;
    private readonly IRepository<Sys_Dept> _repo;

    public Sys_DeptController(IAuth auth, DeptApp app, IRepository<Sys_Dept> repo) : base(auth)
    {
        _app = app;
        _repo = repo;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult List()
    {
        var rows = _repo.Find(null, null, "[ParentId] ASC, [Id] ASC").ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpPost]
    public IActionResult Save([FromBody] Sys_Dept dept)
    {
        if (dept == null || string.IsNullOrWhiteSpace(dept.DeptName))
            return Json(new ResponseData { code = 400, msg = "部门名称必填" });
        var id = _app.Save(dept);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        var hasChildren = _repo.Count("[ParentId]=@p", new { p = id }) > 0;
        if (hasChildren) return Json(new ResponseData { code = 400, msg = "请先删除子部门" });
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
