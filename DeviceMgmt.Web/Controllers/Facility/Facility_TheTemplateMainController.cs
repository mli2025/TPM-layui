using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Validation;
using DeviceMgmt.App.Constants;
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
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.GetTemplateMainList(req));

    [HttpGet]
    public IActionResult GetMainInfo([FromQuery] long Id) => Json(new ResponseData { code = 0, data = _app.Get(Id) });

    [HttpPost]
    public IActionResult SaveTemplate([FromForm] Facility_TheTemplateMain model)
    {
        if (string.IsNullOrWhiteSpace(model.HNumber))
            return Json(new ResponseData { code = 400, msg = "模板编号不能为空" });
        if (string.IsNullOrWhiteSpace(model.HName))
            return Json(new ResponseData { code = 400, msg = "模板名称不能为空" });

        if (!FacilityCategoryType.IsDefined(model.Type))
            model.Type = FacilityCategoryType.Maintenance;

        model.MaintenanceType = TemplateMaintenanceCycle.NormalizeToCode(model.MaintenanceType);
        if (string.IsNullOrWhiteSpace(model.MaintenanceType) || !TemplateMaintenanceCycle.IsValidCode(model.MaintenanceType))
        {
            if (model.Id == 0)
                return Json(new ResponseData { code = 400, msg = "请选择保养周期（年/季/月/周）" });
            var existType = _app.Get(model.Id);
            var prev = TemplateMaintenanceCycle.NormalizeToCode(existType?.MaintenanceType);
            if (!string.IsNullOrWhiteSpace(prev) && TemplateMaintenanceCycle.IsValidCode(prev))
                model.MaintenanceType = prev;
            else
                return Json(new ResponseData { code = 400, msg = "请选择保养周期（年/季/月/周）" });
        }

        model.Hdate ??= DateTime.Now;
        model.CheckDate ??= DateTime.Now;
        model.Status ??= 1;
        model.FGC_LastModifyDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        model.FGC_CreateDate ??= DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

        var makerName = CurrentUser?.User.Name;
        if (string.IsNullOrWhiteSpace(makerName))
            makerName = CurrentUser?.User.Account;
        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Maker))
                model.Maker = makerName;
            _app.Add(model);
        }
        else
        {
            var existing = _app.Get(model.Id);
            if (existing != null && string.IsNullOrWhiteSpace(model.Maker))
                model.Maker = existing.Maker ?? makerName;
            _app.Update(model);
        }

        return Json(new ResponseData { code = 0, msg = "ok", data = model.Id });
    }

    [HttpPost]
    public IActionResult DeleteTemplate([FromForm] long id)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "参数错误" });
        var subs = _subApp.GetByMainId(id).Select(x => x.Id).ToArray();
        if (subs.Length > 0) _subApp.Delete(subs);
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpGet]
    public IActionResult GetTemplateSubs([FromQuery] long mainId)
    {
        var rows = _subApp.GetByMainId(mainId).ToList();
        return Json(new TableData { code = 0, count = rows.Count, data = rows });
    }

    [HttpPost]
    public IActionResult SaveTemplateSub([FromForm] Facility_TheTemplateSub model)
    {
        if (model.MainId <= 0) return Json(new ResponseData { code = 400, msg = "主表ID不能为空" });
        if (string.IsNullOrWhiteSpace(model.HContent)) return Json(new ResponseData { code = 400, msg = "项目不能为空" });
        model.HMethods ??= string.Empty;
        model.HStandard ??= string.Empty;
        var rangeErr = InspectControlValidator.ValidateNumericRange(model.ControlType ?? 0, model.MinValue, model.MaxValue);
        if (rangeErr != null) return Json(new ResponseData { code = 400, msg = rangeErr });
        if (model.Id == 0) _subApp.Add(model);
        else _subApp.Update(model);
        return Json(new ResponseData { code = 0, msg = "ok", data = model.Id });
    }

    [HttpPost]
    public IActionResult DeleteTemplateSub([FromForm] long id)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "参数错误" });
        _subApp.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.GetTemplateMainList(req);
        var rows = (pageData.data as IEnumerable<Facility_TheTemplateMain>) ?? Enumerable.Empty<Facility_TheTemplateMain>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Facility_TheTemplateMain");
        return File(bytes, "application/vnd.ms-excel", "Facility_TheTemplateMain.xls");
    }
}