using DeviceMgmt.App.Apps.Maint;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Maint;

/// <summary>维保延期申请审批</summary>
public class Maint_DelayApplyController : BaseController
{
    private readonly Maint_DelayApplyApp _app;
    private readonly IRepository<Facility_BillMain> _billRepo;
    private readonly IRepository<Facility_ResourceDetail> _deviceRepo;

    public Maint_DelayApplyController(IAuth auth, Maint_DelayApplyApp app,
        IRepository<Facility_BillMain> billRepo, IRepository<Facility_ResourceDetail> deviceRepo) : base(auth)
    {
        _app = app;
        _billRepo = billRepo;
        _deviceRepo = deviceRepo;
    }

    public IActionResult Index() => View();

    private static string MaintainTypeText(string? t) => (t ?? "").Trim().ToUpperInvariant() switch
    {
        "YEAR" => "年保",
        "QUARTER" => "季保",
        "MONTH" => "月保",
        "WEEK" => "周保",
        "HALFYEAR" => "半年保",
        _ => string.IsNullOrWhiteSpace(t) ? "" : t!
    };

    /// <summary>列表：业务列展示保养单号 + 设备编码/名称/派工人员/保养类型；仅显示关联工单状态为「新建/已派工(待接单)」的申请。</summary>
    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req)
    {
        req.limit = req.limit <= 0 ? 1000 : Math.Max(req.limit, 1000);
        var page = _app.Getmainlist(req);
        var rows = (page.data as IEnumerable<Maint_DelayApply>)?.ToList() ?? new List<Maint_DelayApply>();

        var bizIds = rows.Select(r => r.BizId).Where(x => x > 0).Distinct().ToArray();
        var billMap = bizIds.Length == 0 ? new Dictionary<long, Facility_BillMain>()
            : _billRepo.Find("[Id] IN @ids", new { ids = bizIds }).GroupBy(b => b.Id).ToDictionary(g => g.Key, g => g.First());
        var devIds = billMap.Values.Where(b => b.FacilityID.HasValue).Select(b => b.FacilityID!.Value).Distinct().ToArray();
        var devMap = devIds.Length == 0 ? new Dictionary<long, Facility_ResourceDetail>()
            : _deviceRepo.Find("[Id] IN @ids", new { ids = devIds }).GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First());

        var data = rows.Select(r =>
        {
            billMap.TryGetValue(r.BizId, out var bill);
            Facility_ResourceDetail? dev = null;
            if (bill?.FacilityID != null) devMap.TryGetValue(bill.FacilityID.Value, out dev);
            return new
            {
                r.Id, r.BizType, r.BizId,
                r.OldDate, r.OldEndDate, r.NewDate, r.NewEndDate, r.Reason, r.ApplyUser, r.ApproveStatus,
                BillNo = bill?.BillNo,
                BillStatus = bill?.Status,
                FacilityCode = dev?.FacilityCode,
                FacilityName = dev?.FacilityName,
                RepairStaff = bill?.RepairStaff,
                MaintainType = MaintainTypeText(bill?.MaintainType)
            };
        })
        // 仅显示关联工单为「新建(0)/已派工待接单(1)」的申请，已完成等不展示
        .Where(x => x.BillStatus == 0 || x.BillStatus == 1)
        .ToList();

        return Json(new TableData { code = 0, count = data.Count, data = data });
    }

    /// <summary>延期申请可选的保养工单（仅新建/已派工状态），含设备编码/名称/派工人员/保养类型/原计划日期段。</summary>
    [HttpPost]
    public IActionResult WorkOrders([FromQuery] string? kw = null)
    {
        var conds = new List<string> { "([BillType] IS NULL OR [BillType] <> 'INSPECTION')", "([Status]=0 OR [Status]=1)" };
        var p = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(kw))
        {
            conds.Add("([BillNo] LIKE @k OR [RepairStaff] LIKE @k)");
            p["k"] = "%" + kw.Trim() + "%";
        }
        var bills = _billRepo.Find(string.Join(" AND ", conds), p, "[Id] DESC").Take(200).ToList();
        var devIds = bills.Where(b => b.FacilityID.HasValue).Select(b => b.FacilityID!.Value).Distinct().ToArray();
        var devMap = devIds.Length == 0 ? new Dictionary<long, Facility_ResourceDetail>()
            : _deviceRepo.Find("[Id] IN @ids", new { ids = devIds }).GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.First());
        var data = bills.Select(b =>
        {
            Facility_ResourceDetail? dev = null;
            if (b.FacilityID != null) devMap.TryGetValue(b.FacilityID.Value, out dev);
            return new
            {
                b.Id, b.BillNo, b.BeginDate, b.EndDate, b.Status,
                FacilityCode = dev?.FacilityCode,
                FacilityName = dev?.FacilityName,
                RepairStaff = b.RepairStaff,
                MaintainType = MaintainTypeText(b.MaintainType)
            };
        }).ToList();
        return Json(new TableData { code = 0, count = data.Count, data = data });
    }

    [HttpPost]
    public IActionResult Save([FromBody] Maint_DelayApply model)
    {
        if (model == null) return Json(new ResponseData { code = 400, msg = "no data" });
        if (model.BizId <= 0) return Json(new ResponseData { code = 400, msg = "请选择保养工单" });
        if (string.IsNullOrWhiteSpace(model.ApplyUser))
            model.ApplyUser = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account;
        model.BizType = "工单";
        var id = _app.SaveApply(model);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Approve([FromForm] long id, [FromForm] bool agree)
    {
        var m = _app.Get(id);
        if (m == null) return Json(new ResponseData { code = 404, msg = "申请不存在" });
        _app.Approve(id, agree);
        // 审批通过：把申请的新日期段写入工单「变更后开始/结束日期」（原计划日期保留）
        if (agree && m.BizId > 0)
        {
            var bill = _billRepo.FindSingle(m.BizId);
            if (bill != null)
            {
                bill.ChangedBeginDate = m.NewDate;
                bill.ChangedEndDate = m.NewEndDate;
                _billRepo.Update(bill);
            }
        }
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        var m = _app.Get(id);
        if (m == null) return Json(new ResponseData { code = 404, msg = "申请不存在" });
        if (m.ApproveStatus != 0) return Json(new ResponseData { code = 400, msg = "已审批的申请不允许删除" });
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
