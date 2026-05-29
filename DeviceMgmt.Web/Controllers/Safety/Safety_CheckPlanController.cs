using System.Globalization;
using DeviceMgmt.App.Apps.Safety;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Safety;

public class Safety_CheckPlanController : BaseController
{
    private readonly Safety_CheckPlanApp _app;
    private readonly IRepository<Safety_Accessory> _accRepo;

    public Safety_CheckPlanController(IAuth auth, Safety_CheckPlanApp app, IRepository<Safety_Accessory> accRepo) : base(auth)
    {
        _app = app;
        _accRepo = accRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Save([FromBody] Safety_CheckPlan e)
    {
        if (e == null || e.AccId == 0) return Json(new ResponseData { code = 400, msg = "请选择安全附件" });
        if (e.Id == 0) _app.Add(e); else _app.Update(e);
        return Json(new ResponseData { code = 0, data = e.Id.ToString(), msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }

    [HttpGet]
    public IActionResult GetAccessories()
    {
        var rows = _accRepo.Find("[Status]=1", null, "[AccCode] ASC")
            .Select(x => new { Id = x.Id.ToString(CultureInfo.InvariantCulture), x.AccCode, x.Model }).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }
}
