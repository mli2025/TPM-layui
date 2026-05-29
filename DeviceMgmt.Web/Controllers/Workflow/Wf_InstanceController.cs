using System.Globalization;
using DeviceMgmt.App.Apps.Workflow;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Wf;

public class Wf_InstanceController : BaseController
{
    private readonly WorkflowApp _app;
    private readonly IRepository<Wf_Instance> _instRepo;
    private readonly IRepository<Wf_Template> _tplRepo;

    public Wf_InstanceController(IAuth auth, WorkflowApp app, IRepository<Wf_Instance> instRepo, IRepository<Wf_Template> tplRepo) : base(auth)
    {
        _app = app;
        _instRepo = instRepo;
        _tplRepo = tplRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] int? status)
    {
        var where = status.HasValue ? "[Status]=@s" : null;
        var rows = _instRepo.Find(where, status.HasValue ? new { s = status.Value } : null, "[Id] DESC").ToList();
        return Json(new TableData { code = 0, count = rows.Count, data = rows });
    }

    [HttpGet]
    public IActionResult GetDetail([FromQuery] long id)
    {
        var inst = _app.GetInstance(id);
        if (inst == null) return Json(new ResponseData { code = 404, msg = "not found" });
        var logs = _app.GetLogs(id).ToList();
        var tpl = _app.GetTemplate(inst.TemplateId);
        var nodes = _app.GetNodes(inst.TemplateId).ToList();
        return Json(new ResponseData { code = 0, data = new { instance = inst, logs, template = tpl, nodes } });
    }

    [HttpGet]
    public IActionResult GetTemplates()
    {
        var rows = _app.ActiveTemplates()
            .Select(x => new { Id = x.Id.ToString(CultureInfo.InvariantCulture), x.Code, x.Name, x.Module }).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpPost]
    public IActionResult Start([FromBody] StartWfReq req)
    {
        if (req == null || req.TemplateId == 0) return Json(new ResponseData { code = 400, msg = "请选择流程模板" });
        var uid = CurrentUser?.User?.Id ?? 0;
        var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? "system";
        var id = _app.Start(req.TemplateId, req.BizType ?? "manual", req.BizId, uid, name);
        return Json(new ResponseData { code = 0, data = id.ToString(CultureInfo.InvariantCulture), msg = "ok" });
    }

    [HttpPost]
    public IActionResult Approve([FromForm] long id, [FromForm] string? opinion)
    {
        var uid = CurrentUser?.User?.Id ?? 0;
        var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? "system";
        try { _app.Approve(id, uid, name, opinion); return Json(new ResponseData { code = 0, msg = "ok" }); }
        catch (InvalidOperationException ex) { return Json(new ResponseData { code = 400, msg = ex.Message }); }
    }

    [HttpPost]
    public IActionResult Reject([FromForm] long id, [FromForm] string? opinion)
    {
        var uid = CurrentUser?.User?.Id ?? 0;
        var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? "system";
        try { _app.Reject(id, uid, name, opinion); return Json(new ResponseData { code = 0, msg = "ok" }); }
        catch (InvalidOperationException ex) { return Json(new ResponseData { code = 400, msg = ex.Message }); }
    }

    [HttpPost]
    public IActionResult Withdraw([FromForm] long id)
    {
        var uid = CurrentUser?.User?.Id ?? 0;
        try { _app.Withdraw(id, uid); return Json(new ResponseData { code = 0, msg = "ok" }); }
        catch (InvalidOperationException ex) { return Json(new ResponseData { code = 400, msg = ex.Message }); }
    }
}

public class StartWfReq
{
    public long TemplateId { get; set; }
    public string? BizType { get; set; }
    public long BizId { get; set; }
}
