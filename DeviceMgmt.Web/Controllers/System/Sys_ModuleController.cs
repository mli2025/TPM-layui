using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

public class Sys_ModuleController : BaseController
{
    private readonly ModuleApp _app;
    private readonly IRepository<Sys_Module> _repo;
    private readonly IRepository<Sys_RoleModule> _rmRepo;

    public Sys_ModuleController(
        IAuth auth,
        ModuleApp app,
        IRepository<Sys_Module> repo,
        IRepository<Sys_RoleModule> rmRepo) : base(auth)
    {
        _app = app;
        _repo = repo;
        _rmRepo = rmRepo;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult List()
    {
        var rows = _repo.Find(null, null, "[ParentId] ASC, [Sort] ASC, [Id] ASC").ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpGet]
    public IActionResult GetModule([FromQuery] long id)
    {
        var m = _app.Get(id);
        if (m == null) return Json(new ResponseData { code = 404, msg = "not found" });
        return Json(new ResponseData { code = 0, data = m });
    }

    [HttpPost]
    public IActionResult Save([FromBody] Sys_Module m)
    {
        if (m == null || string.IsNullOrWhiteSpace(m.Name))
            return Json(new ResponseData { code = 400, msg = "名称必填" });
        var id = _app.SaveModule(m);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        var hasChildren = _repo.Count("[ParentId]=@p", new { p = id }) > 0;
        if (hasChildren) return Json(new ResponseData { code = 400, msg = "请先删除子模块" });
        var rmIds = _rmRepo.Find("[ModuleId]=@m", new { m = id }).Select(x => x.Id).ToArray();
        if (rmIds.Length > 0) _rmRepo.Delete(rmIds);
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpGet]
    public IActionResult GetButtons([FromQuery] long moduleId)
    {
        return Json(new ResponseData { code = 0, data = _app.GetButtons(moduleId) });
    }

    [HttpPost]
    public IActionResult SaveButton([FromBody] Sys_ModuleButtons b)
    {
        if (b == null || string.IsNullOrWhiteSpace(b.Name)) return Json(new ResponseData { code = 400, msg = "按钮名称必填" });
        if (string.IsNullOrWhiteSpace(b.DomId)) return Json(new ResponseData { code = 400, msg = "DomId 必填" });
        var id = _app.SaveButton(b);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult DeleteButton([FromForm] long id)
    {
        _app.DeleteButton(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
