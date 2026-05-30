using DeviceMgmt.App.Apps.Archive;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Archive;

/// <summary>FAT/SAT 验收</summary>
public class Facility_AcceptanceController : BaseController
{
    private readonly Facility_AcceptanceApp _app;
    public Facility_AcceptanceController(IAuth auth, Facility_AcceptanceApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult GetDetail([FromQuery] long id) => Json(new ResponseData { code = 0, data = new { Main = _app.Get(id), Issues = _app.GetIssues(id) } });
    [HttpPost]
    public IActionResult Save([FromBody] SaveReq req)
    {
        if (req?.Main == null) return Json(new ResponseData { code = 400, msg = "no data" });
        return Json(new ResponseData { code = 0, data = _app.Save(req.Main, req.Issues), msg = "ok" });
    }
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.DeleteCascade(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
    public class SaveReq { public Facility_Acceptance? Main { get; set; } public List<Facility_AcceptanceIssue>? Issues { get; set; } }
}

/// <summary>设备盘点</summary>
public class Facility_StockCheckController : BaseController
{
    private readonly Facility_StockCheckApp _app;
    public Facility_StockCheckController(IAuth auth, Facility_StockCheckApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult GetDetail([FromQuery] long id) => Json(new ResponseData { code = 0, data = new { Main = _app.Get(id), Subs = _app.GetSubs(id) } });
    [HttpPost]
    public IActionResult Save([FromBody] SaveReq req)
    {
        if (req?.Main == null) return Json(new ResponseData { code = 400, msg = "no data" });
        return Json(new ResponseData { code = 0, data = _app.Save(req.Main, req.Subs), msg = "ok" });
    }
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.DeleteCascade(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
    public class SaveReq { public Facility_StockCheck? Main { get; set; } public List<Facility_StockCheckSub>? Subs { get; set; } }
}

/// <summary>资产卡片</summary>
public class Facility_AssetCardController : BaseController
{
    private readonly Facility_AssetCardApp _app;
    public Facility_AssetCardController(IAuth auth, Facility_AssetCardApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult GetInfo([FromQuery] long id) => Json(new ResponseData { code = 0, data = _app.Get(id) });
    [HttpPost] public IActionResult Save([FromBody] Facility_AssetCard m) => m == null ? Json(new ResponseData { code = 400, msg = "no data" }) : Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}

/// <summary>证书/许可时效</summary>
public class Facility_CertController : BaseController
{
    private readonly Facility_CertApp _app;
    public Facility_CertController(IAuth auth, Facility_CertApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult GetInfo([FromQuery] long id) => Json(new ResponseData { code = 0, data = _app.Get(id) });
    [HttpPost] public IActionResult Save([FromBody] Facility_Cert m) => m == null ? Json(new ResponseData { code = 400, msg = "no data" }) : Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}

/// <summary>设备标签（二维码/条码）</summary>
public class Facility_LabelController : BaseController
{
    private readonly Facility_LabelApp _app;
    public Facility_LabelController(IAuth auth, Facility_LabelApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpPost] public IActionResult Save([FromBody] Facility_Label m) => m == null ? Json(new ResponseData { code = 400, msg = "no data" }) : Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}

/// <summary>润滑标准</summary>
public class Facility_LubeStandardController : BaseController
{
    private readonly Facility_LubeStandardApp _app;
    public Facility_LubeStandardController(IAuth auth, Facility_LubeStandardApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult GetInfo([FromQuery] long id) => Json(new ResponseData { code = 0, data = _app.Get(id) });
    [HttpPost] public IActionResult Save([FromBody] Facility_LubeStandard m) => m == null ? Json(new ResponseData { code = 400, msg = "no data" }) : Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}

/// <summary>润滑记录</summary>
public class Facility_LubeRecordController : BaseController
{
    private readonly Facility_LubeRecordApp _app;
    public Facility_LubeRecordController(IAuth auth, Facility_LubeRecordApp app) : base(auth) { _app = app; }
    public IActionResult Index() => View();
    [HttpPost] public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
    [HttpGet] public IActionResult GetInfo([FromQuery] long id) => Json(new ResponseData { code = 0, data = _app.Get(id) });
    [HttpPost] public IActionResult Save([FromBody] Facility_LubeRecord m) => m == null ? Json(new ResponseData { code = 400, msg = "no data" }) : Json(new ResponseData { code = 0, data = _app.Save(m), msg = "ok" });
    [HttpPost] public IActionResult Delete([FromForm] long id) { _app.Delete(id); return Json(new ResponseData { code = 0, msg = "ok" }); }
}
