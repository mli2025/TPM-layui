using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using DeviceMgmt.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

public class Sys_SettingController : BaseController
{
    private readonly SettingService _setting;
    private readonly OperationLogService _opLog;
    private readonly ILogger<Sys_SettingController> _logger;

    public Sys_SettingController(IAuth auth, SettingService setting, OperationLogService opLog, ILogger<Sys_SettingController> logger) : base(auth)
    {
        _setting = setting;
        _opLog = opLog;
        _logger = logger;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult List([FromQuery] string? group)
    {
        var rows = _setting.GetAll();
        if (!string.IsNullOrEmpty(group)) rows = rows.Where(x => x.Group == group);
        return Json(new ResponseData { code = 0, data = rows.ToList() });
    }

    [HttpGet]
    public IActionResult Groups()
    {
        var groups = _setting.GetAll().Select(x => x.Group).Distinct().ToList();
        return Json(new ResponseData { code = 0, data = groups });
    }

    /// <summary>批量保存</summary>
    [HttpPost]
    public IActionResult Save([FromBody] Dictionary<string, string?> payload)
    {
        if (payload == null || payload.Count == 0)
            return Json(new ResponseData { code = 400, msg = "no values" });
        var n = _setting.UpdateMany(payload);
        _setting.Reload();
        _logger.LogInformation("Sys_Setting updated {Count} keys by {User}", n, CurrentUser?.User?.Account);
        _opLog.Write("setting.update", "system", $"updated {n} keys",
            userId: CurrentUser?.User?.Id, userAccount: CurrentUser?.User?.Account);
        return Json(new ResponseData { code = 0, data = n, msg = "ok" });
    }

    [HttpGet]
    public IActionResult Reload()
    {
        _setting.Reload();
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
