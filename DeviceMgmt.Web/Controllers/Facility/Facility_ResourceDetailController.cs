using System.Data;
using DeviceMgmt.App.Apps.Basic;
using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

public class Facility_ResourceDetailController : BaseController
{
    private readonly Facility_ResourceDetailApp _app;
    private readonly EmployeeApp _empApp;
    private readonly IRepository<Facility_TheTemplateMain> _tplRepo;
    private readonly IRepository<Facility_BillMain> _billRepo;

    public Facility_ResourceDetailController(
        IAuth auth,
        Facility_ResourceDetailApp app,
        EmployeeApp empApp,
        IRepository<Facility_TheTemplateMain> tplRepo,
        IRepository<Facility_BillMain> billRepo) : base(auth)
    {
        _app = app;
        _empApp = empApp;
        _tplRepo = tplRepo;
        _billRepo = billRepo;
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
        return Json(new ResponseData { code = 0, data = data });
    }

    [HttpGet]
    public IActionResult GetTemplateOptions([FromQuery] int type, [FromQuery] string? maintenanceType = null)
    {
        var where = "[Type]=@t";
        object param;
        if (!string.IsNullOrEmpty(maintenanceType))
        {
            where += " AND [MaintenanceType]=@mt";
            param = new { t = type, mt = maintenanceType };
        }
        else
        {
            param = new { t = type };
        }
        var rows = _tplRepo.Find(where, param, "[Id] DESC")
            .Select(x => new { Id = x.Id, HNumber = x.HNumber, HName = x.HName, MaintenanceType = x.MaintenanceType })
            .ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpGet]
    public IActionResult GetFacilityHistory([FromQuery] long facilityId, [FromQuery] string? billType = null)
    {
        var where = "[FacilityID]=@fid";
        object param;
        if (!string.IsNullOrEmpty(billType))
        {
            where += " AND [BillType]=@bt";
            param = new { fid = facilityId, bt = billType };
        }
        else
        {
            param = new { fid = facilityId };
        }
        var rows = _billRepo.Find(where, param, "[Id] DESC").ToList();
        return Json(new TableData { code = 0, count = rows.Count, data = rows });
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
