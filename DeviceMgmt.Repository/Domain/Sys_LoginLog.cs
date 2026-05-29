using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>登录日志（URS 401-410）</summary>
[Table("Sys_LoginLog")]
public class Sys_LoginLog : Entity
{
    public long? UserId { get; set; }
    public string? Account { get; set; }
    public DateTime LoginTime { get; set; } = DateTime.Now;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; } = true;
    public string? FailReason { get; set; }
}

/// <summary>账户锁定（连续失败阈值锁定，仅管理员解锁）</summary>
[Table("Sys_AccountLock")]
public class Sys_AccountLock : Entity
{
    public long UserId { get; set; }
    public string? Account { get; set; }
    public int FailCount { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public string? UnlockedBy { get; set; }
    public bool IsLocked { get; set; }
}
