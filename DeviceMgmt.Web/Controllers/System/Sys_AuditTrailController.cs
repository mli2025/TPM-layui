using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

/// <summary>
/// 字段级审计追踪查询（只读）。数据由 AuditService.WriteDiff 写入。
/// </summary>
public class Sys_AuditTrailController : BaseController
{
    private readonly AuditTrailApp _app;

    public Sys_AuditTrailController(IAuth auth, AuditTrailApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    /// <summary>某条业务记录的变更时间线</summary>
    [HttpGet]
    public IActionResult GetTimeline([FromQuery] string targetType, [FromQuery] string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetType) || string.IsNullOrWhiteSpace(targetId))
            return Json(new ResponseData { code = 400, msg = "参数缺失" });
        return Json(new ResponseData { code = 0, data = _app.GetTimeline(targetType, targetId) });
    }
}
