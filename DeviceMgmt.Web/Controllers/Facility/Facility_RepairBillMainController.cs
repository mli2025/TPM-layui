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

    public Facility_RepairBillMainController(
        IAuth auth,
        Facility_RepairBillMainApp app,
        IRepository<Basic_Employee> empRepo,
        IRepository<Facility_ResourceDetail> deviceRepo) : base(auth)
    {
        _app = app;
        _empRepo = empRepo;
        _deviceRepo = deviceRepo;
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
        var uid = CurrentUser?.User?.Id ?? 0;
        var id = _app.SaveBill(req.Main, req.Subs ?? new List<Facility_RepairBillSub>(), uid);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Dispatch([FromForm] long id, [FromForm] string repairStaff)
    {
        if (string.IsNullOrWhiteSpace(repairStaff)) return Json(new ResponseData { code = 400, msg = "请选择维修人员" });
        var uid = CurrentUser?.User?.Id ?? 0;
        var dispatchName = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? uid.ToString();
        var (ok, msg) = _app.Dispatch(id, repairStaff, uid, dispatchName);
        return Json(new ResponseData { code = ok ? 0 : 400, msg = msg });
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
        var rows = _empRepo.Find(where, param, "[Id] DESC")
            .Select(e => new { Id = e.Id, Name = e.Name, EmployeeNumber = e.EmployeeNumber, DeptId = e.DeptId })
            .ToList();
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
