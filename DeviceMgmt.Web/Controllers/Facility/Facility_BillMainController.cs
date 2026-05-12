using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

public class Facility_BillMainController : BaseController
{
    private readonly Facility_BillMainApp _app;

    public Facility_BillMainController(IAuth auth, Facility_BillMainApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();
    public IActionResult ViewList(long id) { ViewBag.Id = id; return View(); }

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetMainInfo([FromQuery] long Id) => Json(new ResponseData { code = 0, data = _app.Get(Id) });

    [HttpGet]
    public IActionResult NextBillNo()
    {
        var n = "BY" + DateTime.Now.ToString("yyyyMMddHHmmss") + Random.Shared.Next(100, 999);
        return Json(new ResponseData { code = 0, data = n });
    }

    [HttpPost]
    public IActionResult SaveMain([FromForm] Facility_BillMain model)
    {
        if (string.IsNullOrWhiteSpace(model.BillNo))
            return Json(new ResponseData { code = 400, msg = "单据编号不能为空" });

        var uid = CurrentUser?.User.Id ?? 0L;
        var now = DateTime.Now;
        model.CreateUserId = uid;
        model.LastUpdateUserId = uid;
        model.CheckerUserId = uid;
        model.CreateDate = now;
        model.LastUpdateDate = now;
        model.BillDate ??= now;
        model.BillType = string.IsNullOrWhiteSpace(model.BillType) ? "MAINTENANCE" : model.BillType.Trim();
        model.Status ??= 0;

        if (model.Id == 0) _app.Add(model);
        else _app.Update(model);
        return Json(new ResponseData { code = 0, msg = "ok", data = model.Id });
    }

    [HttpPost]
    public IActionResult DeleteMain([FromForm] long id)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "参数错误" });
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Facility_BillMain>) ?? Enumerable.Empty<Facility_BillMain>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Facility_BillMain");
        return File(bytes, "application/vnd.ms-excel", "Facility_BillMain.xls");
    }
}