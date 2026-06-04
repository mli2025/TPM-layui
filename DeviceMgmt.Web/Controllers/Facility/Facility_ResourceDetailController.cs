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
using DeviceMgmt.Web.Services.Import;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Facility;

public class Facility_ResourceDetailController : BaseController
{
    private readonly Facility_ResourceDetailApp _app;
    private readonly EmployeeApp _empApp;
    private readonly IRepository<Facility_TheTemplateMain> _tplRepo;
    private readonly IRepository<Facility_BillMain> _billRepo;
    private readonly IRepository<Sys_Dept> _deptRepo;
    private readonly IRepository<Basic_Resource> _resourceRepo;
    private readonly ImportService _import;
    private readonly IRepository<Inspect_Record> _inspectRecordRepo;
    private readonly IRepository<Basic_Employee> _empRepo;
    private readonly IRepository<Sys_User> _userRepo;

    public Facility_ResourceDetailController(
        IAuth auth,
        Facility_ResourceDetailApp app,
        EmployeeApp empApp,
        IRepository<Facility_TheTemplateMain> tplRepo,
        IRepository<Facility_BillMain> billRepo,
        IRepository<Sys_Dept> deptRepo,
        IRepository<Basic_Resource> resourceRepo,
        ImportService import,
        IRepository<Inspect_Record> inspectRecordRepo,
        IRepository<Basic_Employee> empRepo,
        IRepository<Sys_User> userRepo) : base(auth)
    {
        _app = app;
        _empApp = empApp;
        _tplRepo = tplRepo;
        _billRepo = billRepo;
        _deptRepo = deptRepo;
        _resourceRepo = resourceRepo;
        _import = import;
        _inspectRecordRepo = inspectRecordRepo;
        _empRepo = empRepo;
        _userRepo = userRepo;
    }

    /// <summary>把工号/用户Id 解析为姓名（已是姓名则原样返回）。</summary>
    private string ResolvePerson(string? raw, Dictionary<string, string> empByNo, Dictionary<string, string> userById)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var k = raw.Trim();
        if (empByNo.TryGetValue(k, out var n1)) return n1;
        if (userById.TryGetValue(k, out var n2)) return n2;
        return k;
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
        if (data == null) return Json(new ResponseData { code = 404, msg = "记录不存在" });
        string? deptName = null;
        if (data.DeptId > 0)
        {
            var dept = _deptRepo.FindSingle(data.DeptId);
            deptName = dept?.DeptName;
        }
        string? resourceDisplay = null;
        if (data.ResourceId > 0)
        {
            try
            {
                var res = _resourceRepo.FindSingle(data.ResourceId);
                if (res != null) resourceDisplay = (res.Code ?? "") + " / " + (res.Name ?? "");
            }
            catch { /* Basic_Resource 表可能不存在于部分环境 */ }
        }
        return Json(new ResponseData
        {
            code = 0,
            data = new
            {
                data.Id,
                data.FacilityCode,
                data.FacilityName,
                data.FacilityType,
                data.ResourceId,
                ResourceDisplay = resourceDisplay,
                data.Manufacturer,
                data.Supplier,
                data.ManufacturerDate,
                data.ManufactureCountry,
                data.Model,
                data.ExpireDate,
                data.PurchasePrice,
                data.PurchaseDate,
                data.SerialNumber,
                data.EquipmentManual,
                data.EquipmentDrawing,
                data.Location,
                data.DeptId,
                DeptName = deptName,
                data.AssetNumber,
                data.Voltage,
                data.Size,
                data.Weight,
                data.The5STemplateMainId,
                data.TheTemplateMainId,
                data.UseCondition,
                data.LastCheckDate,
                data.NextCheckDate,
                data.LastRepairDate,
                data.AssetManager,
                data.FacilitySign,
                data.Continuous_WorkTime,
                data.RunTime,
                data.ElectrifyTime,
                data.Continuous_Qty,
                data.Status,
                data.InWarehouseUserId,
                data.InWarehouseDate,
                data.CreateDate,
                data.CreateUserId,
                data.TerminalAddress,
                data.FormulaIds,
                data.MonthTempId,
                data.SeasonTempId,
                data.HalfYearTempId,
                data.WeekTempId,
                data.YearTempId,
                data.LastMonthMainTainDate,
                data.LastYSeasonMainTainDate,
                data.LastHalfYearMainTainDate,
                data.LastYearMainTainDate,
                data.Type,
                data.Standard,
                data.Keeper,
                data.MonthPlanDay,
                data.MonthWeek,
                data.Remark,
                data.AcceptanceDate,
                data.NWXCode,
                data.KeyFlag,
                data.StandardYears,
                data.EntityId,
                data.ManufactureNumber,
                data.EquipmentBodyNumber,
                data.MeasurementRange,
                data.Resolution,
                data.Accuracy,
                data.CalibrationDate,
                data.CalibrationCycle,
                data.CalibrationExpirationDate,
                data.CalibrationExpirationWarningDays,
                data.Custodian,
                data.ActualValue
            }
        });
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

    /// <summary>设备保养记录（保养人/审核人解析为姓名，默认执行日期倒序）。</summary>
    [HttpGet]
    public IActionResult GetFacilityHistory([FromQuery] long facilityId, [FromQuery] string? billType = null)
    {
        var where = "[FacilityID]=@fid AND ([BillType] IS NULL OR [BillType] <> 'INSPECTION')";
        object param = new { fid = facilityId };
        if (!string.IsNullOrEmpty(billType) && billType != "INSPECTION")
        {
            where = "[FacilityID]=@fid AND [BillType]=@bt";
            param = new { fid = facilityId, bt = billType };
        }
        var rows = _billRepo.Find(where, param, "[Id] DESC").ToList();

        var empByNo = _empRepo.Find(null, null).Where(e => !string.IsNullOrWhiteSpace(e.EmployeeNumber))
            .GroupBy(e => e.EmployeeNumber).ToDictionary(g => g.Key, g => g.First().Name);
        var userById = _userRepo.Find(null, null)
            .GroupBy(u => u.Id.ToString()).ToDictionary(g => g.Key, g => g.First().Name ?? g.First().Account);

        var data = rows
            .OrderByDescending(r => r.EndDate ?? r.BillDate ?? r.CreateDate)
            .Select(r => new
            {
                r.Id, r.BillNo, r.BillDate, r.BeginDate, r.EndDate, r.MaintainType, r.Status, r.CheckDate, r.Remark,
                RepairStaff = ResolvePerson(r.RepairStaff, empByNo, userById),
                Checker = ResolvePerson(r.Checker, empByNo, userById)
            }).ToList();
        return Json(new TableData { code = 0, count = data.Count, data = data });
    }

    /// <summary>设备点检记录（来自 Inspect_Record，默认执行日期倒序）。</summary>
    [HttpGet]
    public IActionResult GetFacilityCheckHistory([FromQuery] long facilityId)
    {
        var rows = _inspectRecordRepo.Find("[FacilityId]=@fid", new { fid = facilityId })
            .OrderByDescending(r => r.ExecTime ?? r.PlanDate ?? r.CreateDate)
            .Select(r => new
            {
                r.Id, r.RecordNo, r.PlanDate, r.Shift, r.Executor, r.ExecTime, r.Result, r.Remark
            }).ToList();
        return Json(new TableData { code = 0, count = rows.Count, data = rows });
    }

    [HttpPost]
    public IActionResult SaveFacility([FromForm] Facility_ResourceDetail entity)
    {
        var res = new ResponseData();
        try
        {
            var isNew = entity.Id == 0;
            var err = FacilityResourceDetailSaveHelper.Validate(entity, isNew);
            if (err != null) return Json(new ResponseData { code = 400, msg = err });
            FacilityResourceDetailSaveHelper.Normalize(entity);
            if (isNew) _app.Add(entity);
            else _app.Update(entity);
        }
        catch (Exception ex)
        {
            res.code = 500;
            res.msg = FacilityResourceDetailSaveHelper.ToFriendlyMessage(ex);
        }
        return Json(res);
    }

    [HttpGet]
    public IActionResult ImportTemplate()
    {
        var bytes = _import.BuildTemplate(FacilityImportHandler.BizTypeConst);
        if (bytes == null) return Json(new ResponseData { code = 400, msg = "导入模板不可用" });
        return File(bytes, "application/vnd.ms-excel", "设备台账导入模板.xls");
    }

    [HttpPost]
    public IActionResult ImportExcel(IFormFile? file)
    {
        if (file == null || file.Length == 0) return Json(new ResponseData { code = 400, msg = "请选择 Excel 文件" });
        using var stream = file.OpenReadStream();
        var result = _import.Import(FacilityImportHandler.BizTypeConst, stream, file.FileName, CurrentUser?.User?.Id);
        return Json(new ResponseData
        {
            code = 0,
            msg = "ok",
            data = new { result.Total, result.Success, result.Fail, result.Skip, errors = result.Errors.Take(50) }
        });
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
