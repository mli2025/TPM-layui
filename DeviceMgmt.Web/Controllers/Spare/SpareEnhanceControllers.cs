using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Spare;

/// <summary>备件预警配置 + 预警/可出库量</summary>
public class Spare_AlarmController : BaseController
{
    private readonly Spare_AlarmConfigApp _app;
    public Spare_AlarmController(IAuth auth, Spare_AlarmConfigApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult AlarmList() => Json(new ResponseData { code = 0, data = _app.AlarmList() });
    [HttpGet] public IActionResult AvailableList() => Json(new ResponseData { code = 0, data = _app.AvailableList() });
    [HttpPost] public IActionResult Save([FromBody] Spare_AlarmConfig m) => m == null ? Json(new ResponseData { code = 400, msg = "no data" }) : Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}

/// <summary>备件生命周期</summary>
public class Spare_LifeCycleController : BaseController
{
    private readonly Spare_LifeCycleApp _app;
    public Spare_LifeCycleController(IAuth auth, Spare_LifeCycleApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult GetBySpare([FromQuery] long spareId) => Json(new ResponseData { code = 0, data = _app.GetBySpare(spareId) });
    [HttpPost]
    public IActionResult Save([FromBody] Spare_LifeCycle m)
    {
        if (m == null) return Json(new ResponseData { code = 400, msg = "no data" });
        if (string.IsNullOrWhiteSpace(m.Operator)) m.Operator = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account;
        return Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    }
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}

/// <summary>备件盘点</summary>
public class Spare_StockCheckController : BaseController
{
    private readonly Spare_StockCheckApp _app;
    public Spare_StockCheckController(IAuth auth, Spare_StockCheckApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult GetDetail([FromQuery] long id) => Json(new ResponseData { code = 0, data = new { Main = _app.Get(id), Subs = _app.GetSubs(id) } });
    [HttpPost]
    public IActionResult Save([FromBody] SaveReq req)
    {
        if (req?.Main == null) return Json(new ResponseData { code = 400, msg = "no data" });
        return Json(new ResponseData { code = 0, data = _app.Save(req.Main, req.Subs), msg = "ok" });
    }
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.DeleteCascade(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
    public class SaveReq { public Spare_StockCheck? Main { get; set; } public List<Spare_StockCheckSub>? Subs { get; set; } }
}
