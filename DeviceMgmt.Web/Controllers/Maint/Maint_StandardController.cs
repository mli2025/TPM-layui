using DeviceMgmt.App.Apps.Maint;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Maint;

/// <summary>维保标准库（主子）</summary>
public class Maint_StandardController : BaseController
{
    private readonly Maint_StandardApp _app;

    public Maint_StandardController(IAuth auth, Maint_StandardApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetDetail([FromQuery] long id)
        => Json(new ResponseData { code = 0, data = new { Main = _app.Get(id), Subs = _app.GetSubs(id) } });

    [HttpPost]
    public IActionResult Save([FromBody] StandardSaveReq req)
    {
        if (req?.Main == null) return Json(new ResponseData { code = 400, msg = "no data" });
        var id = _app.Save(req.Main, req.Subs);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        _app.DeleteCascade(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    public class StandardSaveReq
    {
        public Maint_Standard? Main { get; set; }
        public List<Maint_StandardSub>? Subs { get; set; }
    }
}
