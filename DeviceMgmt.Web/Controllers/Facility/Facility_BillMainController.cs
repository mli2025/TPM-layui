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

public class Facility_BillMainController : BaseController
{
    private readonly Facility_BillMainApp _app;
    private readonly IRepository<Facility_ResourceDetail> _deviceRepo;
    private readonly IRepository<Facility_TheTemplateMain> _tplMainRepo;

    public Facility_BillMainController(
        IAuth auth,
        Facility_BillMainApp app,
        IRepository<Facility_ResourceDetail> deviceRepo,
        IRepository<Facility_TheTemplateMain> tplMainRepo) : base(auth)
    {
        _app = app;
        _deviceRepo = deviceRepo;
        _tplMainRepo = tplMainRepo;
    }

    public IActionResult Index() => View();
    public IActionResult ViewList(long id) { ViewBag.Id = id; return View(); }

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetMainInfo([FromQuery] long Id) => Json(new ResponseData { code = 0, data = _app.Get(Id) });

    [HttpGet]
    public IActionResult GetBillDetail([FromQuery] long id)
    {
        var detail = _app.GetWithSubs(id);
        if (detail == null) return Json(new ResponseData { code = 404, msg = "保养单不存在" });
        return Json(new ResponseData { code = 0, data = detail });
    }

    [HttpPost]
    public IActionResult GetPlanDevices([FromForm] PageReq req)
    {
        var pageData = _app.Getmainlist(req);
        var devicePage = new PageReq { page = req.page, limit = req.limit, searchParam = req.searchParam, sfield = req.sfield, sorder = req.sorder };
        var (list, total) = QueryDevices(req);

        var tplIds = list.SelectMany(d => new[] { d.MonthTempId, d.SeasonTempId, d.WeekTempId, d.YearTempId })
                         .Where(x => x.HasValue && x.Value > 0)
                         .Select(x => x!.Value)
                         .Distinct()
                         .ToArray();
        var tplDict = tplIds.Length == 0
            ? new Dictionary<long, Facility_TheTemplateMain>()
            : _tplMainRepo.Find("[Id] IN @ids", new { ids = tplIds }).ToDictionary(x => x.Id);

        var rows = list.Select(d => new
        {
            Id = d.Id,
            FacilityCode = d.FacilityCode,
            FacilityName = d.FacilityName,
            FacilityType = d.FacilityType,
            Model = d.Model,
            MonthTempId = d.MonthTempId,
            MonthTempName = d.MonthTempId.HasValue && tplDict.TryGetValue(d.MonthTempId.Value, out var m) ? m.HName : null,
            SeasonTempId = d.SeasonTempId,
            SeasonTempName = d.SeasonTempId.HasValue && tplDict.TryGetValue(d.SeasonTempId.Value, out var q) ? q.HName : null,
            WeekTempId = d.WeekTempId,
            WeekTempName = d.WeekTempId.HasValue && tplDict.TryGetValue(d.WeekTempId.Value, out var w) ? w.HName : null,
            YearTempId = d.YearTempId,
            YearTempName = d.YearTempId.HasValue && tplDict.TryGetValue(d.YearTempId.Value, out var y) ? y.HName : null,
            LastMonthMainTainDate = d.LastMonthMainTainDate,
            LastYSeasonMainTainDate = d.LastYSeasonMainTainDate,
            LastYearMainTainDate = d.LastYearMainTainDate
        });

        return Json(new TableData { code = 0, count = total, data = rows });
    }

    [HttpPost]
    public IActionResult CheckTemplates([FromForm] long[] deviceIds, [FromForm] string cycle)
    {
        var list = _app.CheckTemplates(deviceIds ?? Array.Empty<long>(), cycle ?? string.Empty);
        return Json(new ResponseData { code = 0, data = list });
    }

    [HttpPost]
    public IActionResult BatchGenerate([FromForm] long[] deviceIds, [FromForm] string cycle, [FromForm] int count, [FromForm] DateTime startDate)
    {
        var uid = CurrentUser?.User?.Id ?? 0;
        var result = _app.BatchGenerate(deviceIds ?? Array.Empty<long>(), cycle ?? string.Empty, count, startDate, uid);
        return Json(new ResponseData
        {
            code = result.Success ? 0 : 400,
            msg = result.Message,
            data = result
        });
    }

    [HttpPost]
    public IActionResult UpdateBill([FromForm] Facility_BillMain model)
    {
        var (ok, msg) = _app.UpdateBillWithGuard(model);
        return Json(new ResponseData { code = ok ? 0 : 400, msg = msg });
    }

    [HttpPost]
    public IActionResult DeleteBill([FromForm] long id)
    {
        var (ok, msg) = _app.DeleteBillWithGuard(id);
        return Json(new ResponseData { code = ok ? 0 : 400, msg = msg });
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

    private (List<Facility_ResourceDetail> data, int total) QueryDevices(PageReq req)
    {
        var filters = (req.searchParam ?? new List<searchParam>())
            .Where(x => !string.IsNullOrWhiteSpace(x?.value))
            .Select(x => new Infrastructure.Filter
            {
                field = x.field,
                Value = x.value,
                conditional = string.IsNullOrEmpty(x.conditional) ? "like" : x.conditional
            })
            .ToArray();
        var (rows, total) = _deviceRepo.FindPaged(filters, req.page, req.limit, "[Id] DESC");
        return (rows.ToList(), total);
    }
}
