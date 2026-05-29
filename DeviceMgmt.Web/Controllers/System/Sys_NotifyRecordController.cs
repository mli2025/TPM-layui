using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

/// <summary>
/// 通知中心：当前用户站内消息列表 + 已读标记。
/// </summary>
public class Sys_NotifyRecordController : BaseController
{
    private readonly NotifyApp _app;

    public Sys_NotifyRecordController(IAuth auth, NotifyApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req)
    {
        var uid = CurrentUser?.User?.Id ?? 0;
        // 仅查看本人或广播（ReceiverId 为空）消息
        req.searchParam ??= new List<searchParam>();
        req.searchParam.Add(new searchParam { field = "ReceiverId", conditional = "eq", value = uid.ToString() });
        return Json(_app.Getmainlist(req));
    }

    [HttpGet]
    public IActionResult UnreadCount()
    {
        var uid = CurrentUser?.User?.Id ?? 0;
        return Json(new ResponseData { code = 0, data = _app.UnreadCount(uid) });
    }

    [HttpPost]
    public IActionResult MarkRead([FromForm] long id)
    {
        _app.MarkRead(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult MarkAllRead()
    {
        var uid = CurrentUser?.User?.Id ?? 0;
        _app.MarkAllRead(uid);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
