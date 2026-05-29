using System.Globalization;
using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

/// <summary>
/// 用户组管理：组 CRUD + 成员绑定 + 菜单授权（权限并集叠加）。
/// </summary>
public class Sys_UserGroupController : BaseController
{
    private readonly UserGroupApp _app;
    private readonly IRepository<Sys_Module> _moduleRepo;
    private readonly IRepository<Sys_User> _userRepo;

    public Sys_UserGroupController(
        IAuth auth,
        UserGroupApp app,
        IRepository<Sys_Module> moduleRepo,
        IRepository<Sys_User> userRepo) : base(auth)
    {
        _app = app;
        _moduleRepo = moduleRepo;
        _userRepo = userRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetGroup([FromQuery] long id)
    {
        var g = _app.Get(id);
        if (g == null) return Json(new ResponseData { code = 404, msg = "not found" });
        // long[] 经 JSON 会失真，统一转字符串数组
        var moduleIds = _app.GetGroupModuleIds(id).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
        var userIds = _app.GetGroupUserIds(id).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
        return Json(new ResponseData { code = 0, data = new { group = g, moduleIds, userIds } });
    }

    [HttpPost]
    public IActionResult SaveGroup([FromBody] SaveUserGroupReq req)
    {
        if (req?.Group == null) return Json(new ResponseData { code = 400, msg = "参数为空" });
        if (string.IsNullOrWhiteSpace(req.Group.Name)) return Json(new ResponseData { code = 400, msg = "用户组名称必填" });
        var id = _app.SaveGroup(req.Group);
        _app.SetGroupModules(id, req.ModuleIds ?? Array.Empty<long>());
        _app.SetGroupUsers(id, req.UserIds ?? Array.Empty<long>());
        return Json(new ResponseData { code = 0, data = id.ToString(CultureInfo.InvariantCulture), msg = "ok" });
    }

    [HttpPost]
    public IActionResult DeleteGroup([FromForm] long id)
    {
        _app.DeleteGroupCascade(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpGet]
    public IActionResult GetModules()
    {
        var rows = _moduleRepo.Find(null, null, "[Sort] ASC, [Id] ASC").ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        var rows = _userRepo.Find("[Status]=1", null, "[Account] ASC")
            .Select(u => new { Id = u.Id.ToString(CultureInfo.InvariantCulture), u.Account, u.Name })
            .ToList();
        return Json(new ResponseData { code = 0, data = rows });
    }
}

public class SaveUserGroupReq
{
    public Sys_UserGroup? Group { get; set; }
    public long[]? ModuleIds { get; set; }
    public long[]? UserIds { get; set; }
}
