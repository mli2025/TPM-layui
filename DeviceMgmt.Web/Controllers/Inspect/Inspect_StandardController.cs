using DeviceMgmt.App.Apps.Inspect;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Inspect;

/// <summary>点检标准库（主子）</summary>
public class Inspect_StandardController : BaseController
{
    private readonly Inspect_StandardApp _app;

    public Inspect_StandardController(IAuth auth, Inspect_StandardApp app) : base(auth) { _app = app; }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetDetail([FromQuery] long id)
        => Json(new ResponseData { code = 0, data = new { Main = _app.Get(id), Subs = _app.GetSubs(id) } });

    [HttpGet]
    public IActionResult ActiveList()
        => Json(new ResponseData { code = 0, data = _app.Getmainlist(new PageReq { page = 1, limit = 1000 }).data });

    [HttpPost]
    public IActionResult Save([FromBody] StdSaveReq req)
    {
        if (req?.Main == null) return Json(new ResponseData { code = 400, msg = "no data" });
        var id = _app.Save(req.Main, req.Subs);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.DeleteCascade(id); return Json(new ResponseData { code = 0, msg = "ok" }); }

    public class StdSaveReq
    {
        public Inspect_Standard? Main { get; set; }
        public List<Inspect_StandardSub>? Subs { get; set; }
    }
}
