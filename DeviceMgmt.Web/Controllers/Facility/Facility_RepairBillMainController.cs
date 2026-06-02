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

public class Facility_RepairBillMainController : BaseController
{
    private readonly Facility_RepairBillMainApp _app;
    private readonly IRepository<Basic_Employee> _empRepo;
    private readonly IRepository<Facility_ResourceDetail> _deviceRepo;
    private readonly IRepository<Sys_Dept> _deptRepo;

    public Facility_RepairBillMainController(
        IAuth auth,
        Facility_RepairBillMainApp app,
        IRepository<Basic_Employee> empRepo,
        IRepository<Facility_ResourceDetail> deviceRepo,
        IRepository<Sys_Dept> deptRepo) : base(auth)
    {
        _app = app;
        _empRepo = empRepo;
        _deviceRepo = deviceRepo;
        _deptRepo = deptRepo;
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
        if (detail == null) return Json(new ResponseData { code = 404, msg = "报修单不存在" });
        return Json(new ResponseData { code = 0, data = detail });
    }

    [HttpPost]
    public IActionResult SaveBill([FromBody] SaveRepairBillReq req)
    {
        if (req == null || req.Main == null) return Json(new ResponseData { code = 400, msg = "请求为空" });
        if (req.Main.FacilityId == null || req.Main.FacilityId <= 0)
            return Json(new ResponseData { code = 400, msg = "请选择报修设备" });
        try
        {
            var uid = CurrentUser?.User?.Id ?? 0;
            var id = _app.SaveBill(req.Main, req.Subs ?? new List<Facility_RepairBillSub>(), uid);
            return Json(new ResponseData { code = 0, data = id, msg = "ok" });
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return Json(new ResponseData { code = 500, msg = "保存失败：" + msg });
        }
    }

    [HttpPost]
    public IActionResult Dispatch([FromForm] long id, [FromForm] string repairStaff,
        [FromForm] DateTime? dispatchDate = null,
        [FromForm] DateTime? expectedFinishDate = null,
        [FromForm] string? dispatchRemark = null)
    {
        if (string.IsNullOrWhiteSpace(repairStaff)) return Json(new ResponseData { code = 400, msg = "请选择维修人员" });
        var uid = CurrentUser?.User?.Id ?? 0;
        var dispatchName = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? uid.ToString();
        var (ok, msg) = _app.Dispatch(id, repairStaff, uid, dispatchName, dispatchDate, expectedFinishDate, dispatchRemark);
        return Json(new ResponseData { code = ok ? 0 : 400, msg = msg });
    }

    [HttpPost]
    public IActionResult BatchDispatch([FromForm] string ids, [FromForm] string repairStaff,
        [FromForm] DateTime? dispatchDate = null,
        [FromForm] DateTime? expectedFinishDate = null,
        [FromForm] string? dispatchRemark = null)
    {
        if (string.IsNullOrWhiteSpace(ids)) return Json(new ResponseData { code = 400, msg = "请选择要派工的报修单" });
        if (string.IsNullOrWhiteSpace(repairStaff)) return Json(new ResponseData { code = 400, msg = "请选择维修人员" });
        var idArr = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => long.TryParse(s, out var v) ? v : 0L).Where(v => v > 0).ToArray();
        if (idArr.Length == 0) return Json(new ResponseData { code = 400, msg = "选择项无效" });
        var uid = CurrentUser?.User?.Id ?? 0;
        var dispatchName = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? uid.ToString();
        var (success, fail, errors) = _app.BatchDispatch(idArr, repairStaff, uid, dispatchName, dispatchDate, expectedFinishDate, dispatchRemark);
        var msg = $"派工完成：成功 {success} 单，失败 {fail} 单";
        if (errors.Count > 0) msg += "；详情: " + string.Join(" / ", errors);
        return Json(new ResponseData { code = 0, msg = msg, data = new { success, fail, errors } });
    }

    [HttpPost]
    public IActionResult DeleteBill([FromForm] long id)
    {
        var (ok, msg) = _app.DeleteWithGuard(id);
        return Json(new ResponseData { code = ok ? 0 : 400, msg = msg });
    }

    [HttpGet]
    public IActionResult GetEmployees([FromQuery] string? kw = null)
    {
        var where = "[Status]=1";
        object? param = null;
        if (!string.IsNullOrWhiteSpace(kw))
        {
            where += " AND ([Name] LIKE @k OR [EmployeeNumber] LIKE @k)";
            param = new { k = "%" + kw + "%" };
        }
        var emps = _empRepo.Find(where, param, "[Id] DESC").ToList();
        var deptMap = _deptRepo.Find(null, null, "[Id] ASC").ToDictionary(d => d.Id, d => d.DeptName);
        var loadMap = _app.GetPendingCountByStaff();
        var rows = emps.Select(e => new
        {
            Id = e.Id,
            Name = e.Name,
            EmployeeNumber = e.EmployeeNumber,
            DeptId = e.DeptId,
            DeptName = deptMap.TryGetValue(e.DeptId, out var n) ? n : "",
            PendingCount = loadMap.TryGetValue(e.EmployeeNumber ?? "", out var c) ? c : 0
        }).ToList();
        return Json(new TableData { code = 0, count = rows.Count, data = rows });
    }

    [HttpGet]
    public IActionResult GetDevices([FromQuery] string? kw = null)
    {
        var where = "";
        object? param = null;
        if (!string.IsNullOrWhiteSpace(kw))
        {
            where = "[FacilityCode] LIKE @k OR [FacilityName] LIKE @k OR [Model] LIKE @k";
            param = new { k = "%" + kw + "%" };
        }
        var rows = _deviceRepo.Find(string.IsNullOrEmpty(where) ? null : where, param, "[Id] DESC")
            .Take(200)
            .Select(d => new { Id = d.Id, FacilityCode = d.FacilityCode, FacilityName = d.FacilityName, Model = d.Model })
            .ToList();
        return Json(new TableData { code = 0, count = rows.Count, data = rows });
    }

    /// <summary>设备放大镜分页查询（按编码/名称/型号搜索）</summary>
    [HttpPost]
    public IActionResult GetDevicePickerList()
    {
        var page = int.TryParse(Request.Form["page"], out var pg) && pg > 0 ? pg : 1;
        var limit = int.TryParse(Request.Form["limit"], out var lim) && lim > 0 ? lim : 10;
        var q = Request.Form["query"].ToString();
        if (string.IsNullOrWhiteSpace(q)) q = Request.Form["key"].ToString();
        var idEq = Request.Form["Id"].ToString();

        string where;
        object param;
        if (!string.IsNullOrWhiteSpace(idEq) && long.TryParse(idEq, out var did))
        {
            where = "[Id]=@id";
            param = new { id = did, __skip = (page - 1) * limit, __take = limit };
        }
        else if (!string.IsNullOrWhiteSpace(q))
        {
            where = "([FacilityCode] LIKE @q OR [FacilityName] LIKE @q OR [Model] LIKE @q)";
            param = new { q = "%" + q.Trim() + "%", __skip = (page - 1) * limit, __take = limit };
        }
        else
        {
            where = "1=1";
            param = new { __skip = (page - 1) * limit, __take = limit };
        }

        var total = _deviceRepo.Count(where, param);
        var rows = _deviceRepo.Query<Facility_ResourceDetail>(
            $"SELECT * FROM [Facility_ResourceDetail] WHERE {where} ORDER BY [Id] DESC OFFSET @__skip ROWS FETCH NEXT @__take ROWS ONLY",
            param)
            .Select(d => new { d.Id, d.FacilityCode, d.FacilityName, d.Model, d.DeptId })
            .ToList();
        return Json(new TableData { code = 0, count = total, data = rows });
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Facility_RepairBillMain>) ?? Enumerable.Empty<Facility_RepairBillMain>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Facility_RepairBillMain");
        return File(bytes, "application/vnd.ms-excel", "Facility_RepairBillMain.xls");
    }
}

public class SaveRepairBillReq
{
    public Facility_RepairBillMain? Main { get; set; }
    public List<Facility_RepairBillSub>? Subs { get; set; }
}
