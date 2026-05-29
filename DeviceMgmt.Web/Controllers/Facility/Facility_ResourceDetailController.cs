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
    private readonly IRepository<Facility_ResourceDetail> _resRepo;

    public Facility_ResourceDetailController(
        IAuth auth,
        Facility_ResourceDetailApp app,
        EmployeeApp empApp,
        IRepository<Facility_TheTemplateMain> tplRepo,
        IRepository<Facility_BillMain> billRepo,
        IRepository<Facility_ResourceDetail> resRepo) : base(auth)
    {
        _app = app;
        _empApp = empApp;
        _tplRepo = tplRepo;
        _billRepo = billRepo;
        _resRepo = resRepo;
    }

    public IActionResult Index() => View();
    public IActionResult Index_view() => View();
    public IActionResult ViewList(long id) { ViewBag.Id = id; return View(); }

    // ---- Tabulator 试点（Tailwind + Excel 式表头筛选/排序/服务端分页），不影响原 layui 页 ----

    // 实体真实列名白名单（防止前端传入任意字段拼入 SQL）
    private static readonly HashSet<string> AllowedFields =
        new(typeof(Facility_ResourceDetail).GetProperties().Select(p => p.Name), StringComparer.OrdinalIgnoreCase);

    public IActionResult Grid() => View();

    [HttpPost]
    public IActionResult GridData()
    {
        string Get(string key)
        {
            if (Request.HasFormContentType && Request.Form.ContainsKey(key)) return Request.Form[key].ToString();
            return Request.Query[key].ToString();
        }

        var size = int.TryParse(Get("size"), out var sz) && sz > 0 ? sz : 20;
        var page = int.TryParse(Get("page"), out var pg) && pg > 0 ? pg : 1;

        var req = new PageReq { page = page, limit = size, searchParam = new List<searchParam>() };

        // 排序：sort[0][field] / sort[0][dir]
        var sortField = Get("sort[0][field]");
        if (!string.IsNullOrWhiteSpace(sortField) && AllowedFields.Contains(sortField))
        {
            req.sfield = sortField;
            var dir = Get("sort[0][dir]");
            req.sorder = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
        }

        // 筛选：filter[i][field] / filter[i][type] / filter[i][value]
        for (var i = 0; i < 50; i++)
        {
            var field = Get($"filter[{i}][field]");
            if (string.IsNullOrWhiteSpace(field)) break;
            var value = Get($"filter[{i}][value]");
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (!AllowedFields.Contains(field)) continue;
            req.searchParam!.Add(new searchParam
            {
                field = field,
                conditional = MapFilterType(Get($"filter[{i}][type]")),
                value = value
            });
        }

        var result = _app.Getmainlist(req);
        var total = result.count;
        var lastPage = (int)Math.Ceiling(total / (double)size);
        if (lastPage < 1) lastPage = 1;
        return Json(new { last_page = lastPage, last_row = total, data = result.data });
    }

    // 表头筛选下拉：返回某列去重后的值（已排序），字段必须命中白名单
    [HttpGet]
    public IActionResult DistinctValues([FromQuery] string field)
    {
        if (string.IsNullOrWhiteSpace(field) || !AllowedFields.Contains(field))
            return Json(new ResponseData { code = 0, data = Array.Empty<string>() });

        // field 来自白名单（实体属性名），不含特殊字符，安全拼接
        var sql = $@"SELECT DISTINCT TOP 1000 V FROM
                     (SELECT CAST([{field}] AS NVARCHAR(400)) AS V FROM [Facility_ResourceDetail]) t
                     WHERE V IS NOT NULL AND LTRIM(RTRIM(V)) <> ''
                     ORDER BY V";
        var rows = _resRepo.Query<string>(sql)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    private static string MapFilterType(string? type) => (type ?? "like").ToLowerInvariant() switch
    {
        "=" or "==" => "=",
        "!=" or "<>" => "<>",
        ">" => ">",
        ">=" => ">=",
        "<" => "<",
        "<=" => "<=",
        _ => "like"
    };

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
