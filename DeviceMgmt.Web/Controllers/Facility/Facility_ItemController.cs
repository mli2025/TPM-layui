using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Constants;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

public class Facility_ItemController : BaseController
{
    private readonly Facility_ItemApp _app;
    private readonly IRepository<Facility_TheTemplateSub> _templateSubRepo;

    public Facility_ItemController(IAuth auth, Facility_ItemApp app, IRepository<Facility_TheTemplateSub> templateSubRepo) : base(auth)
    {
        _app = app;
        _templateSubRepo = templateSubRepo;
    }

    public IActionResult Index() => View();
    public IActionResult ViewList(long id) { ViewBag.Id = id; return View(); }

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetMainInfo([FromQuery] long Id) => Json(new ResponseData { code = 0, data = _app.Get(Id) });

    [HttpPost]
    public IActionResult SaveItem([FromForm] Facility_Item model)
    {
        if (string.IsNullOrWhiteSpace(model.Project))
            return Json(new ResponseData { code = 400, msg = "项目名称不能为空" });
        if (string.IsNullOrWhiteSpace(model.CheckMethod))
            model.CheckMethod = string.Empty;
        if (string.IsNullOrWhiteSpace(model.UpkeepMethod))
            model.UpkeepMethod = string.Empty;
        if (string.IsNullOrWhiteSpace(model.FacilityType))
            model.FacilityType = string.Empty;
        if (!FacilityCategoryType.IsDefined(model.Type))
            model.Type = FacilityCategoryType.Maintenance;

        if (model.Id == 0) _app.Add(model);
        else _app.Update(model);
        return Json(new ResponseData { code = 0, msg = "ok", data = model.Id });
    }

    [HttpPost]
    public IActionResult DeleteItem([FromForm] long id)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "参数错误" });
        var usedCount = _templateSubRepo.Count("[HInspectionItemID]=@id", new { id });
        if (usedCount > 0) return Json(new ResponseData { code = 400, msg = "该点检项目已被模板引用，无法删除" });
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Facility_Item>) ?? Enumerable.Empty<Facility_Item>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Facility_Item");
        return File(bytes, "application/vnd.ms-excel", "Facility_Item.xls");
    }

    private static readonly string[] ItemImportHeaders =
        { "项目", "方法", "标准描述", "控件类型(是否/数值)", "最小值", "最大值", "状态(启用/禁用)" };

    [HttpGet]
    public IActionResult ImportItemTemplate()
    {
        var bytes = NPOIHelper.BuildTemplate(ItemImportHeaders, "项目导入模板");
        return File(bytes, "application/vnd.ms-excel", "保养项目导入模板.xls");
    }

    [HttpPost]
    public IActionResult ImportItems(IFormFile? file, [FromForm] int type = 2)
    {
        if (file == null || file.Length == 0) return Json(new ResponseData { code = 400, msg = "请选择 Excel 文件" });
        if (!FacilityCategoryType.IsDefined((short)type)) type = FacilityCategoryType.Maintenance;
        int success = 0, fail = 0;
        var errors = new List<string>();
        using (var stream = file.OpenReadStream())
        {
            var (_, rows) = NPOIHelper.ReadRows(stream, file.FileName);
            var line = 1;
            foreach (var r in rows)
            {
                line++;
                var project = (Get(r, "项目")).Trim();
                if (string.IsNullOrWhiteSpace(project)) { fail++; errors.Add($"第{line}行：项目为空"); continue; }
                var ctText = Get(r, "控件类型(是否/数值)");
                var ct = ctText.Contains("数值") || ctText.Trim() == "1" ? 1 : 0;
                var statusText = Get(r, "状态(启用/禁用)");
                short status = (statusText.Contains("禁") || statusText.Trim() == "0") ? (short)0 : (short)1;
                var item = new Facility_Item
                {
                    Type = (short)type,
                    Project = project,
                    UpkeepMethod = Get(r, "方法"),
                    CheckMethod = Get(r, "标准描述"),
                    ControlType = ct,
                    MinValue = ParseDec(Get(r, "最小值")),
                    MaxValue = ParseDec(Get(r, "最大值")),
                    Status = status,
                    FacilityType = string.Empty
                };
                try { _app.Add(item); success++; }
                catch (Exception ex) { fail++; errors.Add($"第{line}行：{ex.Message}"); }
            }
        }
        return Json(new ResponseData { code = 0, msg = "ok", data = new { success, fail, errors = errors.Take(50) } });
    }

    private static string Get(Dictionary<string, string> r, string key) => r.TryGetValue(key, out var v) ? (v ?? string.Empty) : string.Empty;
    private static decimal? ParseDec(string s) => decimal.TryParse((s ?? string.Empty).Trim(), out var d) ? d : (decimal?)null;
}