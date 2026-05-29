using DeviceMgmt.App.Apps.Special;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Special;

public class Special_EquipmentController : BaseController
{
    private readonly Special_EquipmentApp _app;
    public Special_EquipmentController(IAuth auth, Special_EquipmentApp app) : base(auth) => _app = app;

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Save([FromBody] Special_Equipment e)
    {
        if (e == null || string.IsNullOrWhiteSpace(e.EquipCode))
            return Json(new ResponseData { code = 400, msg = "设备代码必填" });
        try { return Json(new ResponseData { code = 0, data = _app.Save(e).ToString(), msg = "ok" }); }
        catch (InvalidOperationException ex) { return Json(new ResponseData { code = 400, msg = ex.Message }); }
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}
