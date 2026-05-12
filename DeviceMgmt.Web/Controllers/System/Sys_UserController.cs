using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

public class Sys_UserController : BaseController
{
    private readonly UserApp _app;
    private readonly RoleApp _roleApp;
    private readonly IRepository<Sys_Role> _roleRepo;
    private readonly IRepository<Sys_Dept> _deptRepo;

    public Sys_UserController(
        IAuth auth,
        UserApp app,
        RoleApp roleApp,
        IRepository<Sys_Role> roleRepo,
        IRepository<Sys_Dept> deptRepo) : base(auth)
    {
        _app = app;
        _roleApp = roleApp;
        _roleRepo = roleRepo;
        _deptRepo = deptRepo;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpGet]
    public IActionResult GetUser([FromQuery] long id)
    {
        var u = _app.Get(id);
        if (u == null) return Json(new ResponseData { code = 404, msg = "not found" });
        u.Password = string.Empty;
        return Json(new ResponseData { code = 0, data = new { user = u, roles = _roleApp.GetUserRoleIds(id) } });
    }

    [HttpPost]
    public IActionResult SaveUser([FromBody] SaveUserReq req)
    {
        if (req == null || req.User == null) return Json(new ResponseData { code = 400, msg = "参数为空" });
        if (string.IsNullOrWhiteSpace(req.User.Account)) return Json(new ResponseData { code = 400, msg = "账号必填" });
        var id = _app.SaveUser(req.User, req.RawPassword);
        if (req.RoleIds != null) _roleApp.SetUserRoles(id == 0 ? req.User.Id : id, req.RoleIds);
        return Json(new ResponseData { code = 0, data = req.User.Id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult DeleteUser([FromForm] long id)
    {
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult ResetPassword([FromForm] long id, [FromForm] string? newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword)) newPassword = "123456";
        _app.ResetPassword(id, newPassword);
        return Json(new ResponseData { code = 0, msg = "已重置为 " + newPassword });
    }

    [HttpGet]
    public IActionResult GetRoles()
    {
        return Json(new ResponseData { code = 0, data = _roleRepo.Find(null, null, "[Id] DESC").ToList() });
    }

    [HttpGet]
    public IActionResult GetDepts()
    {
        return Json(new ResponseData { code = 0, data = _deptRepo.Find(null, null, "[Id] ASC").ToList() });
    }
}

public class SaveUserReq
{
    public Sys_User? User { get; set; }
    public string? RawPassword { get; set; }
    public long[]? RoleIds { get; set; }
}
