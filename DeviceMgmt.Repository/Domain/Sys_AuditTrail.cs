using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>
/// 审计追踪：字段级变更明细（旧值/新值/理由），配合 Sys_OperationLog 使用。
/// URS 1401-1406 数据完整性 / 变更追溯。
/// </summary>
[Table("Sys_AuditTrail")]
public class Sys_AuditTrail : Entity
{
    public long? LogId { get; set; }
    public long? UserId { get; set; }
    public string? UserAccount { get; set; }
    public string? Module { get; set; }
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public string ActionType { get; set; } = "UPDATE";
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
