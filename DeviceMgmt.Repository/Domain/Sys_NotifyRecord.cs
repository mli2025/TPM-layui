using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>
/// 通知记录（通知引擎落库；多渠道分发经工作流 webhook）
/// </summary>
[Table("Sys_NotifyRecord")]
public class Sys_NotifyRecord : Entity
{
    public long? RuleId { get; set; }
    public string? EventType { get; set; }
    public string? Channel { get; set; }
    public long? ReceiverId { get; set; }
    public string? ReceiverName { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? BizType { get; set; }
    public long? BizId { get; set; }
    public DateTime SendTime { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }
    public DateTime? ReadTime { get; set; }
    public int SendStatus { get; set; }
}
