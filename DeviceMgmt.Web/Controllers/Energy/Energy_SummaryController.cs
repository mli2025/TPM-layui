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

public class Energy_SummaryController : BaseController
{
    private readonly Energy_SummaryApp _app;
    private readonly IRepository<Energy_Point> _pointRepo;

    public Energy_SummaryController(IAuth auth, Energy_SummaryApp app, IRepository<Energy_Point> pointRepo) : base(auth)
    {
        _app = app;
        _pointRepo = pointRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetPoints()
    {
        var rows = _pointRepo.Find(null, null, "[PointCode] ASC")
            .Select(x => new { Id = x.Id.ToString(CultureInfo.InvariantCulture), x.PointCode, x.MediaType }).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }
}
