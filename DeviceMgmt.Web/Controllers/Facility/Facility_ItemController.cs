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
}