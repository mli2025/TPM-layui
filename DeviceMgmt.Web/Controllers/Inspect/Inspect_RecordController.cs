using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Apps.Inspect;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Inspect;

/// <summary>点检执行单（PC 仅查看；逐项明细来自点检模板）</summary>
public class Inspect_RecordController : BaseController
{
    private readonly Inspect_RecordApp _app;
    private readonly Inspect_PlanApp _planApp;
    private readonly Facility_TheTemplateSubApp _tplSubApp;

    public Inspect_RecordController(IAuth auth, Inspect_RecordApp app, Inspect_PlanApp planApp, Facility_TheTemplateSubApp tplSubApp) : base(auth)
    {
        _app = app;
        _planApp = planApp;
        _tplSubApp = tplSubApp;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    /// <summary>把点检模板明细映射为执行单逐项（含控件类型与上下限，供自动判定）</summary>
    private List<Inspect_RecordSub> LoadTemplateItems(long templateId)
        => _tplSubApp.GetByMainId(templateId)
            .Select(s => new Inspect_RecordSub
            {
                ItemName = s.HContent,
                Method = s.HMethods,
                Standard = s.HStandard,
                ControlType = s.ControlType ?? 0,
                MaxValue = s.MaxValue,
                MinValue = s.MinValue
            }).ToList();

    /// <summary>按执行单准备：待执行单从模板带出点检项；已完成单返回已填明细。</summary>
    [HttpGet]
    public IActionResult PrepareByRecord([FromQuery] long recordId)
    {
        var rec = _app.Get(recordId);
        if (rec == null) return Json(new ResponseData { code = 404, msg = "执行单不存在" });
        var subs = _app.GetSubs(recordId);
        if (subs.Count == 0 && rec.PlanId.HasValue)
        {
            var plan = _planApp.Get(rec.PlanId.Value);
            if (plan != null) subs = LoadTemplateItems(plan.TemplateId);
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
        if (id == Inspect_RecordApp.AlreadyDone) return Json(new ResponseData { code = 409, msg = "该点检已被他人完成，无需重复提交" });
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
    public IActionResult Delete([FromForm] long id)
    {
        var rec = _app.Get(id);
        if (rec == null) return Json(new ResponseData { code = 404, msg = "执行单不存在" });
        if (rec.ExecTime != null) return Json(new ResponseData { code = 400, msg = "已执行的点检单不允许删除" });
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    public class RecordSubmitReq
    {
        public Inspect_Record? Main { get; set; }
        public List<Inspect_RecordSub>? Items { get; set; }
    }
}
