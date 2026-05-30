using DeviceMgmt.App.Apps.System;
using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Web.Controllers.Base;
using DeviceMgmt.Web.Services.Import;
using Microsoft.AspNetCore.Mvc;

namespace DeviceMgmt.Web.Controllers.Sys;

/// <summary>通用批量导入：模板下载 + 上传导入 + 导入日志</summary>
public class Sys_ImportController : BaseController
{
    private readonly ImportService _import;
    private readonly ImportLogApp _logApp;

    public Sys_ImportController(IAuth auth, ImportService import, ImportLogApp logApp) : base(auth)
    {
        _import = import;
        _logApp = logApp;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult BizList()
        => Json(new ResponseData { code = 0, data = _import.ListBiz().Select(x => new { bizType = x.BizType, name = x.Name }) });

    [HttpGet]
    public IActionResult Template([FromQuery] string bizType)
    {
        var bytes = _import.BuildTemplate(bizType);
        if (bytes == null) return Json(new ResponseData { code = 400, msg = "不支持的导入类型" });
        return File(bytes, "application/vnd.ms-excel", bizType + "_template.xls");
    }

    [HttpPost]
    public IActionResult Upload([FromForm] string bizType, IFormFile? file)
    {
        if (file == null || file.Length == 0) return Json(new ResponseData { code = 400, msg = "请选择文件" });
        using var stream = file.OpenReadStream();
        var result = _import.Import(bizType, stream, file.FileName, CurrentUser?.User?.Id);
        return Json(new ResponseData
        {
            code = 0,
            msg = "ok",
            data = new { result.Total, result.Success, result.Fail, result.Skip, errors = result.Errors.Take(50) }
        });
    }

    [HttpPost]
    public IActionResult GetLogList([FromForm] PageReq req) => Json(_logApp.Getmainlist(req));
}
