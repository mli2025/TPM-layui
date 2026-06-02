using DeviceMgmt.App.Apps.Inspect;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Inspect;

/// <summary>点检执行单（填写/提交 + 异常处置分流）</summary>
public class Inspect_RecordController : BaseController
{
    private readonly Inspect_RecordApp _app;
    private readonly Inspect_PlanApp _planApp;
    private readonly Inspect_StandardApp _stdApp;

    public Inspect_RecordController(IAuth auth, Inspect_RecordApp app, Inspect_PlanApp planApp, Inspect_StandardApp stdApp) : base(auth)
    {
        _app = app;
        _planApp = planApp;
        _stdApp = stdApp;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    /// <summary>按计划准备执行单：返回计划与标准点检项（供逐项填写）</summary>
    [HttpGet]
    public IActionResult PrepareByPlan([FromQuery] long planId)
    {
        var plan = _planApp.Get(planId);
        if (plan == null) return Json(new ResponseData { code = 404, msg = "计划不存在" });
        var items = _stdApp.GetSubs(plan.StandardId);
        return Json(new ResponseData { code = 0, data = new { plan, items } });
    }

    /// <summary>按执行单准备：待执行单返回标准点检项；已完成单返回已填明细。</summary>
    [HttpGet]
    public IActionResult PrepareByRecord([FromQuery] long recordId)
    {
        var rec = _app.Get(recordId);
        if (rec == null) return Json(new ResponseData { code = 404, msg = "执行单不存在" });
        var subs = _app.GetSubs(recordId);
        if (subs.Count == 0 && rec.PlanId.HasValue)
        {
            var plan = _planApp.Get(rec.PlanId.Value);
            if (plan != null)
                subs = _stdApp.GetSubs(plan.StandardId)
                    .Select(s => new Inspect_RecordSub { ItemName = s.ItemName }).ToList();
        }
        return Json(new ResponseData { code = 0, data = new { record = rec, items = subs } });
    }

    [HttpGet]
    public IActionResult Plans()
        => Json(new ResponseData { code = 0, data = _planApp.Getmainlist(new PageReq { page = 1, limit = 1000 }).data });

    [HttpGet]
    public IActionResult GetDetail([FromQuery] long id)
        => Json(new ResponseData { code = 0, data = new { Main = _app.Get(id), Subs = _app.GetSubs(id), Disposals = _app.GetDisposals(id) } });

    [HttpPost]
    public IActionResult Submit([FromBody] RecordSubmitReq req)
    {
        if (req?.Main == null) return Json(new ResponseData { code = 400, msg = "no data" });
        if (string.IsNullOrWhiteSpace(req.Main.Executor)) req.Main.Executor = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account;
        var id = _app.Submit(req.Main, req.Items);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Dispatch([FromBody] Inspect_Disposal model)
    {
        if (model == null || model.RecordId <= 0) return Json(new ResponseData { code = 400, msg = "参数缺失" });
        var id = _app.Dispatch(model);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }

    public class RecordSubmitReq
    {
        public Inspect_Record? Main { get; set; }
        public List<Inspect_RecordSub>? Items { get; set; }
    }
}
