using DeviceMgmt.App.Interface;
using DeviceMgmt.App.Response;
using DeviceMgmt.Web.Controllers.Base;
using DeviceMgmt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace DeviceMgmt.Web.Controllers.Common;

/// <summary>
/// 通用附件 API：/Sys_Attachment/{Upload|List|Download|Preview|Delete}
/// 通过 (businessType, businessId) 关联到任意业务对象
/// </summary>
public class Sys_AttachmentController : BaseController
{
    private readonly AttachmentService _service;
    private readonly OperationLogService _opLog;
    private readonly ILogger<Sys_AttachmentController> _logger;
    private static readonly FileExtensionContentTypeProvider _mime = new();

    public Sys_AttachmentController(IAuth auth, AttachmentService service, OperationLogService opLog, ILogger<Sys_AttachmentController> logger) : base(auth)
    {
        _service = service;
        _opLog = opLog;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload([FromForm] string businessType, [FromForm] long businessId,
        [FromForm] string? category, IFormFile file)
    {
        if (string.IsNullOrWhiteSpace(businessType) || businessId <= 0)
            return Json(new ResponseData { code = 400, msg = "businessType/businessId 必填" });
        if (file == null) return Json(new ResponseData { code = 400, msg = "no file" });

        try
        {
            var att = await _service.SaveAsync(businessType, businessId, file,
                uploaderId: CurrentUser?.User?.Id,
                uploaderName: CurrentUser?.User?.Account,
                category: category);
            _opLog.Write("attachment.upload", businessType, $"{att.FileName} -> {att.RelativePath}",
                userId: CurrentUser?.User?.Id, userAccount: CurrentUser?.User?.Account,
                targetType: businessType, targetId: businessId.ToString());
            return Json(new ResponseData
            {
                code = 0,
                data = new
                {
                    id = att.Id.ToString(),
                    name = att.FileName,
                    size = att.FileSize,
                    ext = att.FileExt,
                    contentType = att.ContentType,
                    uploadDate = att.UploadDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    uploader = att.UploaderName,
                    url = Url.Action("Preview", new { id = att.Id })
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Attachment upload failed: {Biz}/{Id}", businessType, businessId);
            return Json(new ResponseData { code = -1, msg = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult List([FromQuery] string businessType, [FromQuery] long businessId)
    {
        var list = _service.List(businessType, businessId).Select(a => new
        {
            id = a.Id.ToString(),
            name = a.FileName,
            size = a.FileSize,
            ext = a.FileExt,
            contentType = a.ContentType,
            uploadDate = a.UploadDate.ToString("yyyy-MM-dd HH:mm:ss"),
            uploader = a.UploaderName,
            url = Url.Action("Preview", new { id = a.Id }),
            downloadUrl = Url.Action("Download", new { id = a.Id })
        }).ToList();
        return Json(new ResponseData { code = 0, data = list });
    }

    [HttpGet]
    public IActionResult Download(long id)
    {
        var att = _service.Get(id);
        if (att == null || att.IsDeleted) return NotFound();
        var path = _service.ResolvePath(att);
        if (!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, att.ContentType, att.FileName);
    }

    [HttpGet]
    public IActionResult Preview(long id)
    {
        var att = _service.Get(id);
        if (att == null || att.IsDeleted) return NotFound();
        var path = _service.ResolvePath(att);
        if (!System.IO.File.Exists(path)) return NotFound();
        if (!_mime.TryGetContentType(att.FileName, out var ct)) ct = att.ContentType;
        Response.Headers["Content-Disposition"] = $"inline; filename=\"{Uri.EscapeDataString(att.FileName)}\"";
        return PhysicalFile(path, ct);
    }

    [HttpPost]
    public IActionResult Delete([FromForm] long id)
    {
        var att = _service.Get(id);
        if (att == null) return Json(new ResponseData { code = 404, msg = "not found" });
        _service.SoftDelete(id);
        _opLog.Write("attachment.delete", att.BusinessType, att.FileName,
            userId: CurrentUser?.User?.Id, userAccount: CurrentUser?.User?.Account,
            targetType: att.BusinessType, targetId: att.BusinessId.ToString());
        return Json(new ResponseData { code = 0, msg = "ok" });
    }
}
