using DeviceMgmt.App.Apps.Maint;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Maint;

/// <summary>维保延期申请审批</summary>
public class Maint_DelayApplyController : BaseController
{
    private readonly Maint_DelayApplyApp _app;

    public Maint_DelayApplyController(IAuth auth, Maint_DelayApplyApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Save([FromBody] Maint_DelayApply model)
    {
        if (model == null) return Json(new ResponseData { code = 400, msg = "no data" });
        if (string.IsNullOrWhiteSpace(model.ApplyUser))
            model.ApplyUser = CurrentUser?.User?.Name ?? CurrentUser?.User?.Account;
        var id = _app.SaveApply(model);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Approve([FromForm] long id, [FromForm] bool agree)
    {
        _app.Approve(id, agree);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        var m = _app.Get(id);
        if (m == null) return Json(new ResponseData { code = 404, msg = "申请不存在" });
        if (m.ApproveStatus != 0) return Json(new ResponseData { code = 400, msg = "已审批的申请不允许删除" });
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
