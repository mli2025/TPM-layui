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

public class MeterController : BaseController
{
    private readonly MeterApp _app;
    private readonly IRepository<Sys_Dept> _deptRepo;

    public MeterController(IAuth auth, MeterApp app, IRepository<Sys_Dept> deptRepo) : base(auth)
    {
        _app = app;
        _deptRepo = deptRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Save([FromBody] Meter e)
    {
        if (e == null || string.IsNullOrWhiteSpace(e.MeterCode))
            return Json(new ResponseData { code = 400, msg = "器具编号必填" });
        try { return Json(new ResponseData { code = 0, data = _app.Save(e).ToString(), msg = "ok" }); }
        catch (InvalidOperationException ex) { return Json(new ResponseData { code = 400, msg = ex.Message }); }
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }

    [HttpGet]
    public IActionResult GetDepts()
    {
        var rows = _deptRepo.Find(null, null, "[Id] ASC")
            .Select(x => new { Id = x.Id.ToString(CultureInfo.InvariantCulture), x.DeptName }).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }
}
