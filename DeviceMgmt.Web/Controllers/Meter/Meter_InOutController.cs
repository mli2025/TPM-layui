using System.Globalization;
using DeviceMgmt.App.Apps.Meter;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.MeterMod;

public class Meter_InOutController : BaseController
{
    private readonly Meter_InOutApp _app;
    private readonly IRepository<Meter> _meterRepo;

    public Meter_InOutController(IAuth auth, Meter_InOutApp app, IRepository<Meter> meterRepo) : base(auth)
    {
        _app = app;
        _meterRepo = meterRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Save([FromBody] Meter_InOut e)
    {
        if (e == null || e.MeterId == 0) return Json(new ResponseData { code = 400, msg = "请选择计量器具" });
        if (e.IoTime == default) e.IoTime = System.DateTime.Now;
        if (e.Id == 0) _app.Add(e); else _app.Update(e);
        return Json(new ResponseData { code = 0, data = e.Id.ToString(), msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }

    [HttpGet]
    public IActionResult GetMeters()
    {
        var rows = _meterRepo.Find(null, null, "[MeterCode] ASC")
            .Select(x => new { Id = x.Id.ToString(CultureInfo.InvariantCulture), x.MeterCode, x.Name }).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }
}
