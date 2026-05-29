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

public class Wf_TemplateController : BaseController
{
    private readonly WorkflowApp _app;
    private readonly IRepository<Wf_Template> _tplRepo;

    public Wf_TemplateController(IAuth auth, WorkflowApp app, IRepository<Wf_Template> tplRepo) : base(auth)
    {
        _app = app;
        _tplRepo = tplRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList()
    {
        var rows = _tplRepo.Find(null, null, "[Id] DESC").ToList();
        return Json(new TableData { code = 0, count = rows.Count, data = rows });
    }

    [HttpGet]
    public IActionResult GetTemplate([FromQuery] long id)
    {
        var t = _app.GetTemplate(id);
        if (t == null) return Json(new ResponseData { code = 404, msg = "not found" });
        var nodes = _app.GetNodes(id).ToList();
        return Json(new ResponseData { code = 0, data = new { template = t, nodes } });
    }

    [HttpPost]
    public IActionResult SaveTemplate([FromBody] SaveWfTemplateReq req)
    {
        if (req?.Template == null) return Json(new ResponseData { code = 400, msg = "参数为空" });
        if (string.IsNullOrWhiteSpace(req.Template.Code) || string.IsNullOrWhiteSpace(req.Template.Name))
            return Json(new ResponseData { code = 400, msg = "流程编码与名称必填" });
        try
        {
            var id = _app.SaveTemplate(req.Template, req.Nodes ?? new List<Wf_Node>());
            return Json(new ResponseData { code = 0, data = id.ToString(CultureInfo.InvariantCulture), msg = "ok" });
        }
        catch (InvalidOperationException ex) { return Json(new ResponseData { code = 400, msg = ex.Message }); }
    }

    [HttpPost]
    public IActionResult DeleteTemplate([FromForm] long id)
    {
        _app.DeleteTemplate(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}

public class SaveWfTemplateReq
{
    public Wf_Template? Template { get; set; }
    public List<Wf_Node>? Nodes { get; set; }
}
