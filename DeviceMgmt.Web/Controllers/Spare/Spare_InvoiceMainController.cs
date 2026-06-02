using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Spare;

public class Spare_InvoiceMainController : BaseController
{
    private readonly Spare_InvoiceMainApp _app;
    private readonly IRepository<Basic_Spare> _spareRepo;

    public Spare_InvoiceMainController(IAuth auth, Spare_InvoiceMainApp app, IRepository<Basic_Spare> spareRepo) : base(auth)
    {
        _app = app;
        _spareRepo = spareRepo;
    }

    public IActionResult Index() { ViewBag.TypeName = "全部单据"; return View(); }
    public IActionResult In() { ViewBag.BillType = 1; ViewBag.TypeName = "入库单"; return View("Index"); }
    public IActionResult Out() { ViewBag.BillType = 2; ViewBag.TypeName = "出库单"; return View("Index"); }
    public IActionResult ViewList(long id) { ViewBag.Id = id; return View(); }

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetMainInfo([FromQuery] long Id) => Json(new ResponseData { code = 0, data = _app.Get(Id) });

    [HttpGet]
    public IActionResult GetDetail([FromQuery] long id)
    {
        var d = _app.GetWithSubs(id);
        if (d == null) return Json(new ResponseData { code = 404, msg = "not found" });
        return Json(new ResponseData { code = 0, data = d });
    }

    [HttpPost]
    public IActionResult SaveBill([FromBody] SaveSpareBillReq req)
    {
        if (req == null || req.Main == null) return Json(new ResponseData { code = 400, msg = "参数为空" });
        var uid = CurrentUser?.User?.Id ?? 0;
        var id = _app.SaveInvoice(req.Main, req.Subs ?? new List<Spare_InvoiceSub>(), uid);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Audit([FromForm] long id)
    {
        var uid = CurrentUser?.User?.Id ?? 0;
        var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account;
        var (ok, msg) = _app.Audit(id, uid, name);
        return Json(new ResponseData { code = ok ? 0 : 400, msg = msg });
    }

    [HttpPost]
    public IActionResult DeleteBill([FromForm] long id)
    {
        var (ok, msg) = _app.DeleteWithGuard(id);
        return Json(new ResponseData { code = ok ? 0 : 400, msg = msg });
    }

    [HttpGet]
    public IActionResult GetSpares([FromQuery] string? kw = null)
    {
        var where = "[Status]=1";
        object? param = null;
        if (!string.IsNullOrWhiteSpace(kw))
        {
            where += " AND ([Code] LIKE @k OR [Name] LIKE @k OR [Specs] LIKE @k)";
            param = new { k = "%" + kw + "%" };
        }
        var rows = _spareRepo.Find(where, param, "[Id] DESC")
            .Take(100)
            .Select(s => new { s.Id, s.Code, s.Name, s.Specs, s.Danwei, s.Danjia })
            .ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpPost]
    public IActionResult ExportExcel([FromForm] PageReq req)
    {
        req.page = 1;
        req.limit = int.MaxValue;
        var pageData = _app.Getmainlist(req);
        var rows = (pageData.data as IEnumerable<Spare_InvoiceMain>) ?? Enumerable.Empty<Spare_InvoiceMain>();
        var dt = NPOIHelper.LINQToDataTable(rows);
        var bytes = NPOIHelper.ExportToBytes(dt, "Spare_InvoiceMain");
        return File(bytes, "application/vnd.ms-excel", "Spare_InvoiceMain.xls");
    }
}

public class SaveSpareBillReq
{
    public Spare_InvoiceMain? Main { get; set; }
    public List<Spare_InvoiceSub>? Subs { get; set; }
}
