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

public class Energy_PointController : BaseController
{
    private readonly Energy_PointApp _app;
    private readonly IRepository<Sys_Dept> _deptRepo;

    public Energy_PointController(IAuth auth, Energy_PointApp app, IRepository<Sys_Dept> deptRepo) : base(auth)
    {
        _app = app;
        _deptRepo = deptRepo;
    }

    public IActionResult Index() => View();

    public IActionResult Dashboard() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetDashboard() => Json(new ResponseData { code = 0, data = _app.Dashboard() });

    [HttpPost]
    public IActionResult Save([FromBody] Energy_Point e)
    {
        if (e == null || string.IsNullOrWhiteSpace(e.PointCode))
            return Json(new ResponseData { code = 400, msg = "计量点编号必填" });
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
