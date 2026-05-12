using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

public class Facility_TheTemplateMainController : BaseController
{
    private readonly Facility_TheTemplateMainApp _app;
    private readonly Facility_TheTemplateSubApp _subApp;

    public Facility_TheTemplateMainController(IAuth auth, Facility_TheTemplateMainApp app, Facility_TheTemplateSubApp subApp) : base(auth)
    {
        _app = app;
        _subApp = subApp;
    }

    public IActionResult Index() => View();
    public IActionResult ViewList(long id) { ViewBag.Id = id; return View(); }

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetMainInfo([FromQuery] long Id) => Json(new ResponseData { code = 200, data = _app.Get(Id) });

    [HttpPost]
    public IActionResult SaveTemplate([FromForm] Facility_TheTemplateMain model)
    {
        if (string.IsNullOrWhiteSpace(model.HNumber))
            return Json(new ResponseData { code = 400, msg = "模板编号不能为空" });
        if (string.IsNullOrWhiteSpace(model.HName))
            return Json(new ResponseData { code = 400, msg = "模板名称不能为空" });
        model.Hdate ??= DateTime.Now;
        model.CheckDate ??= DateTime.Now;
        model.Status ??= 1;
        model.Type = model.Type == 0 ? (short)2 : model.Type;
        model.FGC_LastModifyDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        model.FGC_CreateDate ??= DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

        if (model.Id == 0) _app.Add(model);
        else _app.Update(model);

        return Json(new ResponseData { code = 200, msg = "ok", data = model.Id });
    }

    [HttpPost]
    public IActionResult DeleteTemplate([FromForm] long id)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "参数错误" });
        var subs = _subApp.GetByMainId(id).Select(x => x.Id).ToArray();
        if (subs.Length > 0) _subApp.Delete(subs);
        _app.Delete(id);
        return Json(new ResponseData { code = 200, msg = "ok" });
    }

    [HttpGet]
    public IActionResult GetTemplateSubs([FromQuery] long mainId)
    {
        var rows = _subApp.GetByMainId(mainId).ToList();
        return Json(new TableData { code = 200, count = rows.Count, data = rows });
    }

    [HttpPost]
    public IActionResult SaveTemplateSub([FromForm] Facility_TheTemplateSub model)
    {
        if (model.MainId <= 0) return Json(new ResponseData { code = 400, msg = "主表ID不能为空" });
        if (string.IsNullOrWhiteSpace(model.HContent)) return Json(new ResponseData { code = 400, msg = "项目不能为空" });
        model.HMethods ??= string.Empty;
        model.HStandard ??= string.Empty;
        if (model.Id == 0) _subApp.Add(model);
        else _subApp.Update(model);
        return Json(new ResponseData { code = 200, msg = "ok", data = model.Id });
    }

    [HttpPost]
    public IActionResult DeleteTemplateSub([FromForm] long id)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "参数错误" });
        _subApp.Delete(id);
        return Json(new ResponseData { code = 200, msg = "ok" });
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Facility_TheTemplateMain>) ?? Enumerable.Empty<Facility_TheTemplateMain>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Facility_TheTemplateMain");
        return File(bytes, "application/vnd.ms-excel", "Facility_TheTemplateMain.xls");
    }
}