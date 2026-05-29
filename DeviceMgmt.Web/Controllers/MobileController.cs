using DeviceMgmt.App.Apps.Facility;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers;

[Route("m")]
public class MobileController : BaseController
{
    private readonly Facility_BillMainApp _billApp;
    private readonly Facility_RepairBillMainApp _repairApp;
    private readonly IRepository<Facility_BillMain> _billRepo;
    private readonly IRepository<Facility_BillSub> _billSubRepo;
    private readonly IRepository<Facility_RepairBillMain> _repairRepo;
    private readonly IRepository<Facility_RepairBillSub> _repairSubRepo;
    private readonly IRepository<Facility_ResourceDetail> _deviceRepo;
    private readonly IRepository<Basic_Employee> _empRepo;
    private readonly IRepository<Meter> _meterRepo;
    private readonly IRepository<Special_Equipment> _specialRepo;
    private readonly IRepository<Safety_Accessory> _safetyRepo;
    private readonly IWebHostEnvironment _env;

    public MobileController(
        IAuth auth,
        Facility_BillMainApp billApp,
        Facility_RepairBillMainApp repairApp,
        IRepository<Facility_BillMain> billRepo,
        IRepository<Facility_BillSub> billSubRepo,
        IRepository<Facility_RepairBillMain> repairRepo,
        IRepository<Facility_RepairBillSub> repairSubRepo,
        IRepository<Facility_ResourceDetail> deviceRepo,
        IRepository<Basic_Employee> empRepo,
        IRepository<Meter> meterRepo,
        IRepository<Special_Equipment> specialRepo,
        IRepository<Safety_Accessory> safetyRepo,
        IWebHostEnvironment env) : base(auth)
    {
        _billApp = billApp;
        _repairApp = repairApp;
        _billRepo = billRepo;
        _billSubRepo = billSubRepo;
        _repairRepo = repairRepo;
        _repairSubRepo = repairSubRepo;
        _deviceRepo = deviceRepo;
        _empRepo = empRepo;
        _meterRepo = meterRepo;
        _specialRepo = specialRepo;
        _safetyRepo = safetyRepo;
        _env = env;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View("Index");
    }

    [HttpGet("maintain")]
    public IActionResult Maintain()
    {
        ViewBag.PageTitle = "保养待办";
        ViewBag.ActiveTab = "maintain";
        return View("Maintain");
    }

    [HttpGet("maintain/detail")]
    public IActionResult MaintainDetail([FromQuery] long id)
    {
        ViewBag.PageTitle = "保养单详情";
        ViewBag.ActiveTab = "maintain";
        ViewBag.ShowBack = true;
        ViewBag.HideTabBar = true;
        ViewBag.Id = id;
        return View("MaintainDetail");
    }

    [HttpGet("check")]
    public IActionResult Check()
    {
        ViewBag.PageTitle = "点检待办";
        ViewBag.ActiveTab = "check";
        return View("Check");
    }

    [HttpGet("check/detail")]
    public IActionResult CheckDetail([FromQuery] long id)
    {
        ViewBag.PageTitle = "点检单详情";
        ViewBag.ActiveTab = "check";
        ViewBag.ShowBack = true;
        ViewBag.HideTabBar = true;
        ViewBag.Id = id;
        return View("CheckDetail");
    }

    [HttpGet("repair")]
    public IActionResult Repair()
    {
        ViewBag.PageTitle = "维修工单";
        ViewBag.ActiveTab = "repair";
        return View("Repair");
    }

    [HttpGet("repair/create")]
    public IActionResult RepairCreate()
    {
        ViewBag.PageTitle = "我要报修";
        ViewBag.ActiveTab = "repair";
        ViewBag.ShowBack = true;
        ViewBag.HideTabBar = true;
        return View("RepairCreate");
    }

    [HttpGet("repair/detail")]
    public IActionResult RepairDetail([FromQuery] long id)
    {
        ViewBag.PageTitle = "维修单详情";
        ViewBag.ActiveTab = "repair";
        ViewBag.ShowBack = true;
        ViewBag.HideTabBar = true;
        ViewBag.Id = id;
        return View("RepairDetail");
    }

    [HttpGet("asset")]
    public IActionResult Asset()
    {
        ViewBag.PageTitle = "扫码查档案";
        ViewBag.ActiveTab = "home";
        ViewBag.ShowBack = true;
        ViewBag.HideTabBar = true;
        return View("Asset");
    }

    /* ============== APIs ============== */

    [HttpGet("api/devices")]
    public IActionResult ApiDevices([FromQuery] string? kw = null)
    {
        var where = "";
        object? param = null;
        if (!string.IsNullOrWhiteSpace(kw))
        {
            where = "[FacilityCode] LIKE @k OR [FacilityName] LIKE @k OR [Model] LIKE @k";
            param = new { k = "%" + kw + "%" };
        }
        var rows = _deviceRepo.Find(string.IsNullOrEmpty(where) ? null : where, param, "[Id] DESC")
            .Take(50)
            .Select(d => new { d.Id, d.FacilityCode, d.FacilityName, d.Model })
            .ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpGet("api/maintain")]
    public IActionResult ApiMaintainList([FromQuery] string? status = null, [FromQuery] string? kw = null)
    {
        var conds = new List<string> { "([BillType] IS NULL OR [BillType] <> 'INSPECTION')" };
        var p = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(status))
        {
            conds.Add("[Status]=@status");
            p["status"] = int.Parse(status);
        }
        if (!string.IsNullOrWhiteSpace(kw))
        {
            conds.Add("([BillNo] LIKE @k OR [Remark] LIKE @k)");
            p["k"] = "%" + kw + "%";
        }
        var rows = _billRepo.Find(string.Join(" AND ", conds), p, "[Id] DESC").Take(100).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpGet("api/maintain/detail")]
    public IActionResult ApiMaintainDetail([FromQuery] long id)
    {
        var main = _billRepo.FindSingle(id);
        if (main == null) return Json(new ResponseData { code = 404, msg = "not found" });
        var subs = _billSubRepo.Find("[MainId]=@m", new { m = id }, "[Id] ASC").ToList();
        return Json(new ResponseData { code = 0, data = new { main, subs } });
    }

    [HttpPost("api/maintain/submit")]
    public IActionResult ApiMaintainSubmit([FromBody] MaintainSubmitReq req)
    {
        if (req == null || req.Id <= 0) return Json(new ResponseData { code = 400, msg = "参数错误" });
        var main = _billRepo.FindSingle(req.Id);
        if (main == null) return Json(new ResponseData { code = 404, msg = "保养单不存在" });
        var now = DateTime.Now;

        if (req.Items != null)
        {
            foreach (var item in req.Items)
            {
                _billSubRepo.ExecuteSql(
                    "UPDATE [Facility_BillSub] SET [Result]=@r, [Remark]=@k WHERE [Id]=@id AND [MainId]=@mid",
                    new { r = item.Result, k = item.Remark, id = item.Id, mid = req.Id });
            }
        }

        var uid = CurrentUser?.User?.Id ?? 0;
        var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? uid.ToString();
        main.Status = 3;
        main.RepairStaff = main.RepairStaff ?? name;
        main.RepairStaffDate = now;
        main.IsOK = req.IsOK ?? 1;
        main.Remark = string.IsNullOrEmpty(req.Remark) ? main.Remark : req.Remark;
        main.FGC_LastModifier = uid.ToString();
        main.FGC_LastModifyDate = now.ToString("yyyy/MM/dd HH:mm:ss");
        _billRepo.Update(main);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost("api/maintain/dispatch")]
    public IActionResult ApiMaintainDispatch([FromForm] long id)
    {
        var main = _billRepo.FindSingle(id);
        if (main == null) return Json(new ResponseData { code = 404, msg = "保养单不存在" });
        if ((main.Status ?? 0) >= 2) return Json(new ResponseData { code = 400, msg = "已开始/完成的不可重复接单" });
        var uid = CurrentUser?.User?.Id ?? 0;
        var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? uid.ToString();
        main.Status = 2;
        main.RepairStaff = name;
        main.RepairStaffDate = DateTime.Now;
        main.FGC_LastModifier = uid.ToString();
        main.FGC_LastModifyDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        _billRepo.Update(main);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpGet("api/check")]
    public IActionResult ApiCheckList([FromQuery] string? status = null, [FromQuery] string? kw = null)
    {
        var conds = new List<string> { "[BillType]='INSPECTION'" };
        var p = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(status))
        {
            conds.Add("[Status]=@status");
            p["status"] = int.Parse(status);
        }
        if (!string.IsNullOrWhiteSpace(kw))
        {
            conds.Add("([BillNo] LIKE @k OR [Remark] LIKE @k)");
            p["k"] = "%" + kw + "%";
        }
        var rows = _billRepo.Find(string.Join(" AND ", conds), p, "[Id] DESC").Take(100).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpGet("api/repair")]
    public IActionResult ApiRepairList([FromQuery] string? status = null, [FromQuery] string? kw = null, [FromQuery] string? mine = null)
    {
        var conds = new List<string>();
        var p = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(status))
        {
            conds.Add("[Status]=@status");
            p["status"] = int.Parse(status);
        }
        if (!string.IsNullOrWhiteSpace(kw))
        {
            conds.Add("([BillNo] LIKE @k OR [Remark] LIKE @k OR [FaultLocation] LIKE @k)");
            p["k"] = "%" + kw + "%";
        }
        if (mine == "1")
        {
            var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? "";
            conds.Add("([Maker]=@n OR [RepairStaff]=@n)");
            p["n"] = name;
        }
        var rows = _repairRepo.Find(conds.Count == 0 ? null : string.Join(" AND ", conds), p, "[Id] DESC").Take(100).ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpGet("api/repair/detail")]
    public IActionResult ApiRepairDetail([FromQuery] long id)
    {
        var d = _repairApp.GetWithSubs(id);
        if (d == null) return Json(new ResponseData { code = 404, msg = "not found" });
        return Json(new ResponseData { code = 0, data = d });
    }

    [HttpPost("api/repair/create")]
    public IActionResult ApiRepairCreate([FromBody] RepairCreateReq req)
    {
        if (req == null) return Json(new ResponseData { code = 400, msg = "参数错误" });
        var uid = CurrentUser?.User?.Id ?? 0;
        var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? uid.ToString();
        // 把图片 URL 拼到 Remark 末尾，使用 [IMG]url[/IMG] 标记，避免改库结构
        var remark = req.Remark ?? string.Empty;
        if (req.ImageUrls != null && req.ImageUrls.Count > 0)
        {
            var imgs = string.Join("\n", req.ImageUrls.Select(u => $"[IMG]{u}[/IMG]"));
            remark = string.IsNullOrWhiteSpace(remark) ? imgs : remark + "\n" + imgs;
        }
        var main = new Facility_RepairBillMain
        {
            FacilityId = req.FacilityId,
            Descr = req.Descr,
            Remark = remark,
            FaultLocation = req.FaultLocation,
            FaultCategory = req.FaultCategory,
            Maker = name,
            OutsourcingFlag = 0,
            Status = 0
        };
        var subs = new List<Facility_RepairBillSub>();
        if (!string.IsNullOrWhiteSpace(req.ReasonText))
        {
            subs.Add(new Facility_RepairBillSub { Descr = req.ReasonText, Remark = req.Remark });
        }
        var id = _repairApp.SaveBill(main, subs, uid);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost("api/upload")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20MB per upload
    public IActionResult ApiUpload([FromForm] IFormFile? file, [FromForm] string? kind = "repair")
    {
        if (file == null || file.Length == 0) return Json(new ResponseData { code = 400, msg = "未选择文件" });
        var allowExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowExt.Contains(ext)) return Json(new ResponseData { code = 400, msg = "仅支持图片格式 (jpg/png/webp 等)" });

        var safeKind = string.IsNullOrWhiteSpace(kind) ? "misc" : new string(kind.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrEmpty(safeKind)) safeKind = "misc";
        var monthDir = DateTime.Now.ToString("yyyyMM");
        var relDir = $"uploads/{safeKind}/{monthDir}";
        var absDir = System.IO.Path.Combine(_env.WebRootPath ?? "wwwroot", relDir.Replace('/', System.IO.Path.DirectorySeparatorChar));
        System.IO.Directory.CreateDirectory(absDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var absPath = System.IO.Path.Combine(absDir, fileName);
        using (var fs = new System.IO.FileStream(absPath, System.IO.FileMode.CreateNew))
        {
            file.CopyTo(fs);
        }
        var url = "/" + relDir + "/" + fileName;
        return Json(new ResponseData { code = 0, data = new { url, size = file.Length, name = file.FileName }, msg = "ok" });
    }

    /// <summary>扫码/输入编码查档案：依次匹配 计量器具 / 特种设备 / 安全附件</summary>
    [HttpGet("api/asset/by-code")]
    public IActionResult ApiAssetByCode([FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return Json(new ResponseData { code = 400, msg = "code 为空" });
        var c = code.Trim();

        var meter = _meterRepo.Find("[MeterCode]=@c", new { c }, "[Id] DESC").FirstOrDefault();
        if (meter != null)
            return Json(new ResponseData { code = 0, data = new { type = "meter", item = meter } });

        var special = _specialRepo.Find("[EquipCode]=@c", new { c }, "[Id] DESC").FirstOrDefault();
        if (special != null)
            return Json(new ResponseData { code = 0, data = new { type = "special", item = special } });

        var safety = _safetyRepo.Find("[AccCode]=@c", new { c }, "[Id] DESC").FirstOrDefault();
        if (safety != null)
            return Json(new ResponseData { code = 0, data = new { type = "safety", item = safety } });

        return Json(new ResponseData { code = 404, msg = "未找到编码: " + c });
    }

    [HttpGet("api/maintain/by-billno")]
    public IActionResult ApiMaintainByBillNo([FromQuery] string billNo)
    {
        if (string.IsNullOrWhiteSpace(billNo)) return Json(new ResponseData { code = 400, msg = "billNo 为空" });
        var row = _billRepo.Find("[BillNo]=@b", new { b = billNo.Trim() }, "[Id] DESC").FirstOrDefault();
        if (row == null) return Json(new ResponseData { code = 404, msg = "未找到该单号" });
        return Json(new ResponseData { code = 0, data = row });
    }

    [HttpPost("api/repair/accept")]
    public IActionResult ApiRepairAccept([FromForm] long id)
    {
        var m = _repairRepo.FindSingle(id);
        if (m == null) return Json(new ResponseData { code = 404, msg = "not found" });
        if ((m.Status ?? 0) >= 2) return Json(new ResponseData { code = 400, msg = "已开始维修" });
        m.Status = 2;
        m.ResponseDate = DateTime.Now;
        if (string.IsNullOrEmpty(m.RepairStaff))
        {
            m.RepairStaff = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account;
        }
        _repairRepo.Update(m);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost("api/repair/start")]
    public IActionResult ApiRepairStart([FromForm] long id)
    {
        var m = _repairRepo.FindSingle(id);
        if (m == null) return Json(new ResponseData { code = 404, msg = "not found" });
        m.RepairBeginDate = DateTime.Now;
        if ((m.Status ?? 0) < 2) m.Status = 2;
        _repairRepo.Update(m);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost("api/repair/finish")]
    public IActionResult ApiRepairFinish([FromBody] RepairFinishReq req)
    {
        if (req == null || req.Id <= 0) return Json(new ResponseData { code = 400, msg = "参数错误" });
        var m = _repairRepo.FindSingle(req.Id);
        if (m == null) return Json(new ResponseData { code = 404, msg = "not found" });
        m.RepairEndDate = DateTime.Now;
        m.Status = 3;
        var startedAt = m.RepairBeginDate ?? m.ResponseDate ?? m.DispatchDate;
        if (startedAt != null) m.RepairTime = (int)Math.Round((m.RepairEndDate.Value - startedAt.Value).TotalSeconds);
        _repairRepo.Update(m);

        // update first sub: descr/analysis/preventive
        var sub = _repairSubRepo.Find("[MainId]=@m", new { m = req.Id }, "[Id] ASC").FirstOrDefault();
        if (sub == null)
        {
            _repairSubRepo.Insert(new Facility_RepairBillSub
            {
                MainId = req.Id,
                Sort = 1,
                Descr = req.RepairDescr,
                FaultAnalysis = req.FaultAnalysis,
                PreventiveMeasure = req.PreventiveMeasure,
                Remark = req.Remark
            });
        }
        else
        {
            sub.Descr = req.RepairDescr;
            sub.FaultAnalysis = req.FaultAnalysis;
            sub.PreventiveMeasure = req.PreventiveMeasure;
            sub.Remark = req.Remark;
            _repairSubRepo.Update(sub);
        }
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost("api/repair/confirm")]
    public IActionResult ApiRepairConfirm([FromForm] long id, [FromForm] string role)
    {
        var m = _repairRepo.FindSingle(id);
        if (m == null) return Json(new ResponseData { code = 404, msg = "not found" });
        var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? "";
        var now = DateTime.Now;
        switch ((role ?? "").ToLowerInvariant())
        {
            case "equipment":
                m.EquipmentComfirm = name; m.EquipmentComfirmTime = now; break;
            case "produce":
                m.ProduceComfirm = name; m.ProduceComfirmTime = now; break;
            case "quality":
                m.QualityComfirm = name; m.QualityComfirmTime = now; break;
            default:
                return Json(new ResponseData { code = 400, msg = "role 无效" });
        }
        var has = !string.IsNullOrEmpty(m.EquipmentComfirm) && !string.IsNullOrEmpty(m.ProduceComfirm) && !string.IsNullOrEmpty(m.QualityComfirm);
        if (has) m.ComfirmFlag = 1;
        _repairRepo.Update(m);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}

public class MaintainSubmitReq
{
    public long Id { get; set; }
    public int? IsOK { get; set; }
    public string? Remark { get; set; }
    public List<MaintainSubmitItem>? Items { get; set; }
}

public class MaintainSubmitItem
{
    public long Id { get; set; }
    public string? Result { get; set; }
    public string? Remark { get; set; }
}

public class RepairCreateReq
{
    public long? FacilityId { get; set; }
    public string? Descr { get; set; }
    public string? Remark { get; set; }
    public string? FaultLocation { get; set; }
    public string? FaultCategory { get; set; }
    public string? ReasonText { get; set; }
    public List<string>? ImageUrls { get; set; }
}

public class RepairFinishReq
{
    public long Id { get; set; }
    public string? RepairDescr { get; set; }
    public string? FaultAnalysis { get; set; }
    public string? PreventiveMeasure { get; set; }
    public string? Remark { get; set; }
}
