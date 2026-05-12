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

        var uid = CurrentUser?.User?.Id ?? 0L;
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
        var (list, total) = QueryDevices(req);

        var tplIds = list.SelectMany(d => new[] { d.MonthTempId, d.SeasonTempId, d.HalfYearTempId, d.WeekTempId, d.YearTempId })
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
            HalfYearTempId = d.HalfYearTempId,
            HalfYearTempName = d.HalfYearTempId.HasValue && tplDict.TryGetValue(d.HalfYearTempId.Value, out var h) ? h.HName : null,
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
        var raw = (req.searchParam ?? new List<searchParam>())
            .Where(x => !string.IsNullOrWhiteSpace(x?.value))
            .ToList();

        // 与台账列表一致：单关键词对 设备编码/名称/型号/内外校编码 做 OR 模糊查询（原仅 FacilityName 易搜不到）
        if (raw.Count == 1 && string.Equals(raw[0].field, "FacilityName", StringComparison.OrdinalIgnoreCase)
            && string.Equals(raw[0].conditional ?? "like", "like", StringComparison.OrdinalIgnoreCase))
        {
            var kw = "%" + raw[0].value!.Trim() + "%";
            const string where =
                "([FacilityCode] LIKE @kw OR [FacilityName] LIKE @kw OR [Model] LIKE @kw OR ISNULL([NWXCode],'') LIKE @kw)";
            var total = _deviceRepo.Count(where, new { kw });
            var page = Math.Max(1, req.page);
            var limit = Math.Max(1, req.limit);
            var skip = (page - 1) * limit;
            var rows = _deviceRepo.Query<Facility_ResourceDetail>(
                "SELECT * FROM [Facility_ResourceDetail] WHERE " + where + " ORDER BY [Id] DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY",
                new { kw, skip, take = limit });
            return (rows.ToList(), total);
        }

        var filters = raw
            .Select(x => new Infrastructure.Filter
            {
                field = x.field,
                Value = x.value,
                conditional = string.IsNullOrEmpty(x.conditional) ? "like" : x.conditional
            })
            .ToArray();
        var (rows2, total2) = _deviceRepo.FindPaged(filters, req.page, req.limit, "[Id] DESC");
        return (rows2.ToList(), total2);
    }
}
