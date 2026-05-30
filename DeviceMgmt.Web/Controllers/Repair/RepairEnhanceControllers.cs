using DeviceMgmt.App.Apps.Repair;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Repair;

/// <summary>维修工单模板</summary>
public class Facility_RepairTemplateController : BaseController
{
    private readonly Facility_RepairTemplateApp _app;
    public Facility_RepairTemplateController(IAuth auth, Facility_RepairTemplateApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult ActiveList() => Json(new ResponseData { code = 0, data = _app.Getmainlist(new PageReq { page = 1, limit = 1000 }).data });
    [HttpPost] public IActionResult Save([FromBody] Facility_RepairTemplate m) => m == null ? Json(new ResponseData { code = 400, msg = "no data" }) : Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}

/// <summary>维修费用分摊（按工单）</summary>
public class Facility_RepairCostController : BaseController
{
    private readonly Facility_RepairCostApp _app;
    public Facility_RepairCostController(IAuth auth, Facility_RepairCostApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult GetByBill([FromQuery] long billId) => Json(new ResponseData { code = 0, data = _app.GetByBill(billId) });
    [HttpPost]
    public IActionResult SaveBatch([FromBody] CostBatchReq req)
    {
        if (req == null || req.BillId <= 0) return Json(new ResponseData { code = 400, msg = "请指定工单" });
        _app.SaveBatch(req.BillId, req.Rows);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
    public class CostBatchReq { public long BillId { get; set; } public List<Facility_RepairCost>? Rows { get; set; } }
}

/// <summary>报警规则</summary>
public class Facility_AlarmRuleController : BaseController
{
    private readonly Facility_AlarmRuleApp _app;
    public Facility_AlarmRuleController(IAuth auth, Facility_AlarmRuleApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpPost] public IActionResult Save([FromBody] Facility_AlarmRule m) => m == null ? Json(new ResponseData { code = 400, msg = "no data" }) : Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}

/// <summary>报警记录</summary>
public class Facility_AlarmRecordController : BaseController
{
    private readonly Facility_AlarmRecordApp _app;
    public Facility_AlarmRecordController(IAuth auth, Facility_AlarmRecordApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpPost] public IActionResult Save([FromBody] Facility_AlarmRecord m) => m == null ? Json(new ResponseData { code = 400, msg = "no data" }) : Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    [HttpPost]
    public IActionResult Handle([FromForm] long id, [FromForm] string? remark)
    {
        _app.Handle(id, CurrentUser?.User?.Name ?? CurrentUser?.User?.Account, remark);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}

/// <summary>维修故障看板 + 统计分析</summary>
public class Facility_RepairBoardController : BaseController
{
    private readonly RepairStatApp _app;
    public Facility_RepairBoardController(IAuth auth, RepairStatApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpGet] public IActionResult Board() => Json(new ResponseData { code = 0, data = _app.Board() });
    [HttpGet] public IActionResult Trend() => Json(new ResponseData { code = 0, data = _app.MonthlyTrend() });
    [HttpGet] public IActionResult MTTR() => Json(new ResponseData { code = 0, data = _app.MTTRByCategory() });
}
