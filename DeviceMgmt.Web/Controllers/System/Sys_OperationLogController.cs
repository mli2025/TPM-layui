using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

/// <summary>
/// 审计日志查询页（只读）。表与数据由 OperationLogService 写入。
/// </summary>
public class Sys_OperationLogController : BaseController
{
    private readonly OperationLogApp _app;

    public Sys_OperationLogController(IAuth auth, OperationLogApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetDetail([FromQuery] long id)
    {
        var row = _app.Get(id);
        if (row == null) return Json(new ResponseData { code = 404, msg = "记录不存在" });
        return Json(new ResponseData { code = 0, data = row });
    }
}
