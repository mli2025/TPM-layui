using DeviceMgmt.App.Apps.Spare;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Spare;

/// <summary>仓库主数据：备品备件 → 仓库管理</summary>
public class Basic_WarehouseController : BaseController
{
    private readonly Basic_WarehouseApp _app;

    public Basic_WarehouseController(IAuth auth, Basic_WarehouseApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult GetMainList([FromForm] PageReq req) => Json(_app.Getmainlist(req));

    [HttpPost]
    public IActionResult Save([FromBody] Basic_Warehouse model)
    {
        if (model == null) return Json(new ResponseData { code = 400, msg = "no data" });
        if (string.IsNullOrWhiteSpace(model.Code)) return Json(new ResponseData { code = 400, msg = "仓库编码不能为空" });
        if (string.IsNullOrWhiteSpace(model.Name)) return Json(new ResponseData { code = 400, msg = "仓库名称不能为空" });
        var id = _app.Save(model);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        if (id <= 0) return Json(new ResponseData { code = 400, msg = "invalid id" });
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
