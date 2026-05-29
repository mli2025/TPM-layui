using DeviceMgmt.App.Apps.Safety;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Safety;

public class Safety_AccessoryController : BaseController
{
    private readonly Safety_AccessoryApp _app;
    public Safety_AccessoryController(IAuth auth, Safety_AccessoryApp app) : base(auth) => _app = app;

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Save([FromBody] Safety_Accessory e)
    {
        if (e == null || string.IsNullOrWhiteSpace(e.AccCode))
            return Json(new ResponseData { code = 400, msg = "附件编号必填" });
        try { return Json(new ResponseData { code = 0, data = _app.Save(e).ToString(), msg = "ok" }); }
        catch (InvalidOperationException ex) { return Json(new ResponseData { code = 400, msg = ex.Message }); }
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}
