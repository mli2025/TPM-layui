using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

/// <summary>账户锁定管理：列表 + 管理员解锁</summary>
public class Sys_AccountLockController : BaseController
{
    private readonly AccountLockApp _app;
    public Sys_AccountLockController(IAuth auth, AccountLockApp app) : base(auth) => _app = app;

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Unlock([FromForm] long id)
    {
        var name = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account ?? "admin";
        _app.Unlock(id, name);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
