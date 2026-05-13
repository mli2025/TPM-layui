using DeviceMgmt.Repository.Core;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Microsoft.AspNetCore.StaticFiles;

namespace DeviceMgmt.Web.Services;

/// <summary>
/// 通用附件服务：物理文件存储到全局设置中的 storageRoot，DB 仅记录 RelativePath。
/// 这样换路径/换盘只改 Sys_Setting 一处。
/// </summary>
public sealed class AttachmentService
{
    private readonly IRepository<Sys_Attachment> _repo;
    private readonly SettingService _setting;
    private readonly ILogger<AttachmentService> _logger;
    private static readonly FileExtensionContentTypeProvider _mimeProvider = new();

    public AttachmentService(IRepository<Sys_Attachment> repo, SettingService setting, ILogger<AttachmentService> logger)
    {
        _repo = repo;
        _setting = setting;
        _logger = logger;
    }

    public string StorageRoot
    {
        get
        {
            var root = _setting.GetString("storageRoot");
            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.Combine(AppContext.BaseDirectory, "uploads");
            }
            return root!;
        }
    }

    public long MaxFileSize => _setting.GetLong("maxFileSize", 20L * 1024 * 1024);
    public int MaxPerBusiness => _setting.GetInt("maxPerBusiness", 20);

    public IReadOnlyList<string> AllowedExtensions =>
        (_setting.GetString("allowedExt") ?? "")
        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(x => x.TrimStart('.').ToLowerInvariant())
        .Distinct()
        .ToArray();

    public async Task<Sys_Attachment> SaveAsync(string businessType, long businessId, IFormFile file,
        long? uploaderId = null, string? uploaderName = null, string? category = null)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("空文件");
        if (file.Length > MaxFileSize)
            throw new InvalidOperationException($"超过单文件最大限制 {MaxFileSize / 1024 / 1024}MB");

        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = AllowedExtensions;
        if (allowed.Count > 0 && !allowed.Contains(ext))
            throw new InvalidOperationException($"不允许的扩展名: .{ext}");

        var existCount = _repo.Count(
            "[BusinessType]=@bt AND [BusinessId]=@bi AND [IsDeleted]=0",
            new { bt = businessType, bi = businessId });
        if (existCount >= MaxPerBusiness)
            throw new InvalidOperationException($"该单据附件数已达上限 {MaxPerBusiness}");

        var yyyymm = DateTime.Now.ToString("yyyyMM");
        var dir = Path.Combine(StorageRoot, businessType, yyyymm);
        Directory.CreateDirectory(dir);

        var stored = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}.{ext}";
        var fullPath = Path.Combine(dir, stored);
        var relative = Path.Combine(businessType, yyyymm, stored).Replace('\\', '/');

        await using (var fs = File.Create(fullPath))
        {
            await file.CopyToAsync(fs);
        }

        if (!_mimeProvider.TryGetContentType(file.FileName, out var contentType))
            contentType = file.ContentType ?? "application/octet-stream";

        var att = new Sys_Attachment
        {
            BusinessType = businessType,
            BusinessId = businessId,
            FileName = file.FileName,
            StoredName = stored,
            RelativePath = relative,
            ContentType = contentType,
            FileSize = file.Length,
            FileExt = ext,
            Category = category,
            UploaderId = uploaderId,
            UploaderName = uploaderName,
            UploadDate = DateTime.Now
        };
        _repo.Insert(att);
        _logger.LogInformation("Attachment saved {Biz}/{Id} -> {Rel} ({Size} bytes)", businessType, businessId, relative, file.Length);
        return att;
    }

    public IEnumerable<Sys_Attachment> List(string businessType, long businessId)
        => _repo.Find(
            "[BusinessType]=@bt AND [BusinessId]=@bi AND [IsDeleted]=0",
            new { bt = businessType, bi = businessId },
            "[Sort],[Id]");

    public Sys_Attachment? Get(long id) => _repo.FindSingle(id);

    public string ResolvePath(Sys_Attachment att)
        => Path.Combine(StorageRoot, att.RelativePath.Replace('/', Path.DirectorySeparatorChar));

    public int SoftDelete(long id)
    {
        return _repo.ExecuteSql("UPDATE [Sys_Attachment] SET [IsDeleted]=1 WHERE [Id]=@id", new { id });
    }
}
