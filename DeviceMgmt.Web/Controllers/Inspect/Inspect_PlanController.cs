using DeviceMgmt.App.Apps.Inspect;
using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Constants;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Inspect;

/// <summary>点检计划（列表 + 月历）</summary>
public class Inspect_PlanController : BaseController
{
    private readonly Inspect_PlanApp _app;
    private readonly RoleApp _roleApp;
    private readonly IRepository<Facility_TheTemplateMain> _templateRepo;

    public Inspect_PlanController(IAuth auth, Inspect_PlanApp app, RoleApp roleApp, IRepository<Facility_TheTemplateMain> templateRepo) : base(auth)
    {
        _app = app;
        _roleApp = roleApp;
        _templateRepo = templateRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetMonth([FromQuery] int year, [FromQuery] int month)
        => Json(new ResponseData { code = 0, data = _app.GetByMonth(year, month) });

    /// <summary>可选点检模板（Facility_TheTemplateMain，Type=点检，启用）</summary>
    [HttpGet]
    public IActionResult Templates()
    {
        var rows = _templateRepo.Find("[Type]=@t AND ([Status] IS NULL OR [Status]=1)",
                new { t = FacilityCategoryType.Inspection }, "[Id] DESC")
            .Select(x => new { x.Id, x.HName, x.HNumber }).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

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
        if (req?.Main == null || req.Main.TemplateId <= 0) return Json(new ResponseData { code = 400, msg = "请选择点检模板" });
        if (req.Devices == null || req.Devices.Count == 0)
            return Json(new ResponseData { code = 400, msg = "请至少选择一台设备" });
        if (req.RoleIds == null || req.RoleIds.Count == 0)
            return Json(new ResponseData { code = 400, msg = "请至少选择一个执行角色" });
        if (req.Main.PlanDate == null || req.Main.EndDate == null)
            return Json(new ResponseData { code = 400, msg = "请选择起止日期" });
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
