using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using DeviceMgmt.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

public class Sys_VersionController : BaseController
{
    private readonly VersionService _service;
    private readonly OperationLogService _opLog;

    public Sys_VersionController(IAuth auth, VersionService service, OperationLogService opLog) : base(auth)
    {
        _service = service;
        _opLog = opLog;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Current()
    {
        var v = _service.Current();
        return Json(new ResponseData { code = 0, data = v });
    }

    [HttpGet]
    public IActionResult List()
    {
        var rows = _service.Timeline();
        return Json(new ResponseData { code = 0, data = rows.ToList() });
    }

    [HttpPost]
    public IActionResult Save([FromBody] Sys_Version v)
    {
        if (v == null || string.IsNullOrWhiteSpace(v.Version) || string.IsNullOrWhiteSpace(v.Title))
            return Json(new ResponseData { code = 400, msg = "Version/Title 必填" });
        v.Author = CurrentUser?.User?.Account;
        long id;
        if (v.Id > 0) { _service.Update(v); id = v.Id; }
        else id = _service.Publish(v);
        _opLog.Write("version.save", "system", v.Version,
            userId: CurrentUser?.User?.Id, userAccount: CurrentUser?.User?.Account);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        _service.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
