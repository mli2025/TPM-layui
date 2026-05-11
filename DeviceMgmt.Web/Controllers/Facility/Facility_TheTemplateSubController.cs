using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

public class Facility_TheTemplateSubController : BaseController
{
    private readonly Facility_TheTemplateSubApp _app;

    public Facility_TheTemplateSubController(IAuth auth, Facility_TheTemplateSubApp app) : base(auth)
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
        var rows = (pageData.data as IEnumerable<Facility_TheTemplateSub>) ?? Enumerable.Empty<Facility_TheTemplateSub>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Facility_TheTemplateSub");
        return File(bytes, "application/vnd.ms-excel", "Facility_TheTemplateSub.xls");
    }
}