using System.Globalization;
using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

public class Sys_RoleController : BaseController
{
    private readonly RoleApp _app;
    private readonly IRepository<Sys_Module> _moduleRepo;

    public Sys_RoleController(IAuth auth, RoleApp app, IRepository<Sys_Module> moduleRepo) : base(auth)
    {
        _app = app;
        _moduleRepo = moduleRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetRole([FromQuery] long id)
    {
        var r = _app.Get(id);
        if (r == null) return Json(new ResponseData { code = 404, msg = "not found" });
        // 必须返回字符串数组：long[] 在 JSON 里会变成数字，前端超过 2^53-1 会失真，勾选状态对不上
        var moduleIdStrs = _app.GetRoleModuleIds(id).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
        return Json(new ResponseData { code = 0, data = new { role = r, moduleIds = moduleIdStrs } });
    }

    [HttpPost]
    public IActionResult SaveRole([FromBody] SaveRoleReq req)
    {
        if (req == null || req.Role == null) return Json(new ResponseData { code = 400, msg = "参数为空" });
        if (string.IsNullOrWhiteSpace(req.Role.Name)) return Json(new ResponseData { code = 400, msg = "角色名必填" });
        var id = _app.SaveRole(req.Role);
        if (req.ModuleIds != null) _app.SetRoleModules(id == 0 ? req.Role.Id : id, req.ModuleIds);
        return Json(new ResponseData { code = 0, data = req.Role.Id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult DeleteRole([FromForm] long id)
    {
        _app.SetRoleModules(id, Array.Empty<long>());
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpGet]
    public IActionResult GetModules()
    {
        var rows = _moduleRepo.Find(null, null, "[Sort] ASC, [Id] ASC").ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }
}

public class SaveRoleReq
{
    public Sys_Role? Role { get; set; }
    public long[]? ModuleIds { get; set; }
}
