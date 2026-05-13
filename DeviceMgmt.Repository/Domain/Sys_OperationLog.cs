using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>
/// 关键业务操作审计日志（除文件型 Serilog 之外的结构化记录）
/// </summary>
[Table("Sys_OperationLog")]
public class Sys_OperationLog : Entity
{
    public long? UserId { get; set; }
    public string? UserAccount { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
