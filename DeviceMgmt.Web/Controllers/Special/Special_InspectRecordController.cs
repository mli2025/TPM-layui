using System.Globalization;
using DeviceMgmt.App.Apps.Special;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Special;

public class Special_InspectRecordController : BaseController
{
    private readonly Special_InspectRecordApp _app;
    private readonly IRepository<Special_Equipment> _equipRepo;

    public Special_InspectRecordController(IAuth auth, Special_InspectRecordApp app, IRepository<Special_Equipment> equipRepo) : base(auth)
    {
        _app = app;
        _equipRepo = equipRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Save([FromBody] Special_InspectRecord e)
    {
        if (e == null || e.EquipId == 0) return Json(new ResponseData { code = 400, msg = "请选择特种设备" });
        if (e.Id == 0) _app.Add(e); else _app.Update(e);
        return Json(new ResponseData { code = 0, data = e.Id.ToString(), msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }

    [HttpGet]
    public IActionResult GetEquips()
    {
        var rows = _equipRepo.Find("[Status]=1", null, "[EquipCode] ASC")
            .Select(x => new { Id = x.Id.ToString(CultureInfo.InvariantCulture), x.EquipCode, x.Category }).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }
}
