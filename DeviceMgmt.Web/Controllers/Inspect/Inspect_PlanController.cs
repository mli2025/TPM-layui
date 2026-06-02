using DeviceMgmt.App.Apps.Inspect;
using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Inspect;

/// <summary>点检计划（列表 + 月历）</summary>
public class Inspect_PlanController : BaseController
{
    private readonly Inspect_PlanApp _app;
    private readonly Inspect_StandardApp _stdApp;
    private readonly RoleApp _roleApp;

    public Inspect_PlanController(IAuth auth, Inspect_PlanApp app, Inspect_StandardApp stdApp, RoleApp roleApp) : base(auth)
    {
        _app = app;
        _stdApp = stdApp;
        _roleApp = roleApp;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetMonth([FromQuery] int year, [FromQuery] int month)
        => Json(new ResponseData { code = 0, data = _app.GetByMonth(year, month) });

    [HttpGet]
    public IActionResult Standards()
        => Json(new ResponseData { code = 0, data = _stdApp.Getmainlist(new PageReq { page = 1, limit = 1000 }).data });

    [HttpGet]
    public IActionResult GetDevices([FromQuery] long id)
        => Json(new ResponseData { code = 0, data = _app.GetDevices(id) });

    /// <summary>可分配的启用角色列表</summary>
    [HttpGet]
    public IActionResult GetRoles()
        => Json(new ResponseData { code = 0, data = _roleApp.Getmainlist(new PageReq { page = 1, limit = 1000 }).data });

    /// <summary>计划已选角色Id</summary>
    [HttpGet]
    public IActionResult GetRoleIds([FromQuery] long id)
        => Json(new ResponseData { code = 0, data = _app.GetRoleIds(id) });

    [HttpPost]
    public IActionResult Save([FromBody] PlanSaveReq req)
    {
        if (req?.Main == null || req.Main.StandardId <= 0) return Json(new ResponseData { code = 400, msg = "请选择点检标准" });
        if (req.Main.Id == 0 && (req.Devices == null || req.Devices.Count == 0))
            return Json(new ResponseData { code = 400, msg = "请至少选择一台设备" });
        if (req.RoleIds == null || req.RoleIds.Count == 0)
            return Json(new ResponseData { code = 400, msg = "请至少选择一个执行角色" });
        var id = _app.SavePlan(req.Main, req.Devices, req.RoleIds);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }

    public class PlanSaveReq
    {
        public Inspect_Plan? Main { get; set; }
        public List<Inspect_PlanDevice>? Devices { get; set; }
        public List<long>? RoleIds { get; set; }
    }
}
