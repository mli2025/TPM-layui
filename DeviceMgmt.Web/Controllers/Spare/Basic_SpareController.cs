using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Spare;

public class Basic_SpareController : BaseController
{
    private readonly Basic_SpareApp _app;

    public Basic_SpareController(IAuth auth, Basic_SpareApp app) : base(auth)
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
    public IActionResult Save([FromBody] Basic_Spare model, [FromQuery] string? reason)
    {
        if (model == null) return Json(new ResponseData { code = 400, msg = "no data" });
        if (string.IsNullOrWhiteSpace(model.Code)) return Json(new ResponseData { code = 400, msg = "编码不能为空" });

        // 操作理由交由全局审计(Repository层)统一记录
        if (!string.IsNullOrWhiteSpace(reason)) HttpContext.Items["AuditReason"] = reason;
        var id = _app.Save(model);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id, [FromForm] string? reason)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "invalid id" });
        if (!string.IsNullOrWhiteSpace(reason)) HttpContext.Items["AuditReason"] = reason;
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Basic_Spare>) ?? Enumerable.Empty<Basic_Spare>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Basic_Spare");
        return File(bytes, "application/vnd.ms-excel", "Basic_Spare.xls");
    }
}