using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using DeviceMgmt.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Spare;

public class Basic_SpareController : BaseController
{
    private readonly Basic_SpareApp _app;
    private readonly AuditService _audit;

    public Basic_SpareController(IAuth auth, Basic_SpareApp app, AuditService audit) : base(auth)
    {
        _app = app;
        _audit = audit;
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

        var old = model.Id > 0 ? _app.Get(model.Id) : null;
        var id = _app.Save(model);
        _audit.WriteDiff("Basic_Spare", id.ToString(), old, model,
            CurrentUser?.User?.Id, CurrentUser?.User?.Account, "spare", reason);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id, [FromForm] string? reason)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "invalid id" });
        _app.Delete(id);
        _audit.WriteDelete("Basic_Spare", id.ToString(),
            CurrentUser?.User?.Id, CurrentUser?.User?.Account, "spare", reason);
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