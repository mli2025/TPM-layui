using System.Data;
using DeviceMgmt.App.Apps.Basic;
using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

public class Facility_ResourceDetailController : BaseController
{
    private readonly Facility_ResourceDetailApp _app;
    private readonly EmployeeApp _empApp;

    public Facility_ResourceDetailController(IAuth auth, Facility_ResourceDetailApp app, EmployeeApp empApp) : base(auth)
    {
        _app = app;
        _empApp = empApp;
    }

    public IActionResult Index() => View();
    public IActionResult Index_view() => View();
    public IActionResult ViewList(long id) { ViewBag.Id = id; return View(); }

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req)
    {
        var data = _app.Getmainlist(req);
        return Json(data);
    }

    [HttpPost]
    public IActionResult GetMainList_view([FromForm] PageReq req)
    {
        var data = _app.Getmainlist(req);
        return Json(data);
    }

    [HttpGet]
    public IActionResult GetMainInfo([FromQuery] long Id)
    {
        var data = _app.Get(Id);
        return Json(new ResponseData { code = 200, data = data });
    }

    [HttpPost]
    public IActionResult SaveFacility([FromForm] Facility_ResourceDetail entity)
    {
        var res = new ResponseData();
        try
        {
            if (entity.Id == 0) _app.Add(entity);
            else _app.Update(entity);
        }
        catch (Exception ex)
        {
            res.code = 500;
            res.msg = ex.Message;
        }
        return Json(res);
    }

    [HttpPost]
    public IActionResult DeleteFacility([FromForm] long Id)
    {
        var res = new ResponseData();
        try { _app.Delete(Id); }
        catch (Exception ex) { res.code = 500; res.msg = ex.Message; }
        return Json(res);
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Facility_ResourceDetail>) ?? Enumerable.Empty<Facility_ResourceDetail>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Facility_ResourceDetail");
        return File(bytes, "application/vnd.ms-excel", "设备台账.xls");
    }
}
