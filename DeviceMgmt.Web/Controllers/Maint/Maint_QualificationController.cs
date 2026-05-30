using DeviceMgmt.App.Apps.Maint;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Maint;

/// <summary>维保资质有效期监控</summary>
public class Maint_QualificationController : BaseController
{
    private readonly Maint_QualificationApp _app;

    public Maint_QualificationController(IAuth auth, Maint_QualificationApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetInfo([FromQuery] long id) => Json(new ResponseData { code = 0, data = _app.Get(id) });

    [HttpPost]
    public IActionResult Save([FromBody] Maint_Qualification model)
    {
        if (model == null) return Json(new ResponseData { code = 400, msg = "no data" });
        var id = _app.SaveQual(model);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
