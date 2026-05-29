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

public class Meter_SendOutController : BaseController
{
    private readonly Meter_SendOutApp _app;
    private readonly IRepository<Meter> _meterRepo;

    public Meter_SendOutController(IAuth auth, Meter_SendOutApp app, IRepository<Meter> meterRepo) : base(auth)
    {
        _app = app;
        _meterRepo = meterRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetMain([FromQuery] long id)
    {
        var m = _app.Get(id);
        if (m == null) return Json(new ResponseData { code = 404, msg = "not found" });
        var meterIds = _app.GetSubMeterIds(id).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
        return Json(new ResponseData { code = 0, data = new { main = m, meterIds } });
    }

    [HttpPost]
    public IActionResult SaveMain([FromBody] SaveSendOutReq req)
    {
        if (req?.Main == null) return Json(new ResponseData { code = 400, msg = "参数为空" });
        var id = _app.SaveMain(req.Main);
        _app.SetSubMeters(id, req.MeterIds ?? Array.Empty<long>());
        return Json(new ResponseData { code = 0, data = id.ToString(CultureInfo.InvariantCulture), msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.DeleteCascade(id); return Json(new ResponseData { code = 0, msg = "ok" }); }

    [HttpGet]
    public IActionResult GetMeters()
    {
        var rows = _meterRepo.Find("[Status]=1", null, "[MeterCode] ASC")
            .Select(x => new { Id = x.Id.ToString(CultureInfo.InvariantCulture), x.MeterCode, x.Name }).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }
}

public class SaveSendOutReq
{
    public Meter_SendOut? Main { get; set; }
    public long[]? MeterIds { get; set; }
}
