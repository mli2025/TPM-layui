using DeviceMgmt.App.Apps.Mold;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Mold;

public class Mold_TheTemplateMainController : BaseController
{
    private readonly Mold_TheTemplateMainApp _app;

    public Mold_TheTemplateMainController(IAuth auth, Mold_TheTemplateMainApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();
    public IActionResult ViewList(long id) { ViewBag.Id = id; return View(); }

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetMainInfo([FromQuery] long Id) => Json(new ResponseData { code = 200, data = _app.Get(Id) });

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Mold_TheTemplateMain>) ?? Enumerable.Empty<Mold_TheTemplateMain>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Mold_TheTemplateMain");
        return File(bytes, "application/vnd.ms-excel", "Mold_TheTemplateMain.xls");
    }
}