using System.Data;
using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Web.Common;
using DeviceMgmt.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

/// <summary>自定义报表引擎：定义管理 + 运行 + 导出</summary>
public class Sys_ReportController : BaseController
{
    private readonly ReportApp _app;

    public Sys_ReportController(IAuth auth, ReportApp app) : base(auth)
    {
        _app = app;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult List() => Json(new ResponseData { code = 0, data = _app.ListAll() });

    [HttpGet]
    public IActionResult Get([FromQuery] long id) => Json(new ResponseData { code = 0, data = _app.Get(id) });

    [HttpPost]
    public IActionResult Save([FromBody] Sys_ReportDef model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Name))
            return Json(new ResponseData { code = 400, msg = "名称不能为空" });
        var (ok, err) = ReportApp.ValidateSql(model.QueryDef);
        if (!ok) return Json(new ResponseData { code = 400, msg = err });
        model.OwnerId = CurrentUser?.User?.Id;
        var id = _app.Save(model);
        return Json(new ResponseData { code = 0, data = id, msg = "ok" });
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        _app.Delete(id);
        return Json(new ResponseData { code = 0, msg = "ok" });
    }

    [HttpGet]
    public IActionResult Run([FromQuery] long id)
    {
        var (ok, err, rows) = _app.Run(id);
        if (!ok) return Json(new ResponseData { code = 400, msg = err });
        var list = rows.Cast<IDictionary<string, object>>().ToList();
        var columns = list.Count > 0 ? list[0].Keys.ToList() : new List<string>();
        return Json(new ResponseData { code = 0, data = new { columns, rows = list } });
    }

    [HttpGet]
    public IActionResult Export([FromQuery] long id)
    {
        var (ok, err, rows) = _app.Run(id);
        if (!ok) return Json(new ResponseData { code = 400, msg = err });
        var list = rows.Cast<IDictionary<string, object>>().ToList();
        var dt = new DataTable();
        if (list.Count > 0)
            foreach (var k in list[0].Keys) dt.Columns.Add(k, typeof(object));
        foreach (var r in list)
        {
            var dr = dt.NewRow();
            foreach (var k in r.Keys) dr[k] = r[k] ?? DBNull.Value;
            dt.Rows.Add(dr);
        }
        var bytes = NPOIHelper.ExportToBytes(dt, "Report");
        return File(bytes, "application/vnd.ms-excel", "report_" + id + ".xls");
    }
}
