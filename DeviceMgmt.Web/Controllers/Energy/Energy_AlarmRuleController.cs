using System.Globalization;
using DeviceMgmt.App.Apps.Energy;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Energy;

public class Energy_AlarmRuleController : BaseController
{
    private readonly Energy_AlarmRuleApp _app;
    private readonly IRepository<Energy_Point> _pointRepo;

    public Energy_AlarmRuleController(IAuth auth, Energy_AlarmRuleApp app, IRepository<Energy_Point> pointRepo) : base(auth)
    {
        _app = app;
        _pointRepo = pointRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Save([FromBody] Energy_AlarmRule e)
    {
        if (e == null || e.PointId == 0) return Json(new ResponseData { code = 400, msg = "请选择计量点" });
        if (e.Id == 0) _app.Add(e); else _app.Update(e);
        return Json(new ResponseData { code = 0, data = e.Id.ToString(), msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }

    [HttpGet]
    public IActionResult GetPoints()
    {
        var rows = _pointRepo.Find(null, null, "[PointCode] ASC")
            .Select(x => new { Id = x.Id.ToString(CultureInfo.InvariantCulture), x.PointCode, x.MediaType }).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }
}
