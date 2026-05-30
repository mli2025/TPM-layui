using DeviceMgmt.App.Apps.Inspect;
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

    public Inspect_PlanController(IAuth auth, Inspect_PlanApp app, Inspect_StandardApp stdApp) : base(auth)
    {
        _app = app;
        _stdApp = stdApp;
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

    [HttpPost]
    public IActionResult Save([FromBody] Inspect_Plan model)
    {
        if (model == null || model.StandardId <= 0) return Json(new ResponseData { code = 400, msg = "请选择点检标准" });
        var id = _app.SavePlan(model);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}
