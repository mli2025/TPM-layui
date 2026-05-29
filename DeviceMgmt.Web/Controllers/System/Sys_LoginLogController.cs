using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

/// <summary>登录日志查询（只读）</summary>
public class Sys_LoginLogController : BaseController
{
    private readonly LoginLogApp _app;
    public Sys_LoginLogController(IAuth auth, LoginLogApp app) : base(auth) => _app = app;

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));
}
