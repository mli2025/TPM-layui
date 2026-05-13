using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>
/// 通用附件表 - 任意业务对象都可通过 (BusinessType, BusinessId) 挂载
/// </summary>
[Table("Sys_Attachment")]
public class Sys_Attachment : Entity
{
    public string BusinessType { get; set; } = string.Empty;
    public long BusinessId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileExt { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Sort { get; set; }
    public long? UploaderId { get; set; }
    public string? UploaderName { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.Now;
    public string? Remark { get; set; }
    public bool IsDeleted { get; set; }
}
