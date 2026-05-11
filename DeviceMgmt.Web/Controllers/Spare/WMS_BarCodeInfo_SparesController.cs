using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Spare;

public class WMS_BarCodeInfo_SparesController : BaseController
{
    private readonly WMS_BarCodeInfo_SparesApp _app;

    public WMS_BarCodeInfo_SparesController(IAuth auth, WMS_BarCodeInfo_SparesApp app) : base(auth)
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
        var rows = (pageData.data as IEnumerable<WMS_BarCodeInfo_Spares>) ?? Enumerable.Empty<WMS_BarCodeInfo_Spares>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "WMS_BarCodeInfo_Spares");
        return File(bytes, "application/vnd.ms-excel", "WMS_BarCodeInfo_Spares.xls");
    }
}