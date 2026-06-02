namespace DeviceMgmt.Repository.Interface;

/// <summary>
/// 审计上下文：向数据层提供当前操作者 / IP / 模块 / 操作理由，
/// 供 Repository 在增删改时自动写入字段级审计（URS 301-306：全局、自动、不可关闭）。
/// 由宿主(Web)按请求实现；无请求上下文时各属性返回 null。
/// </summary>
public interface IAuditContext
{
    long? UserId { get; }
    string? UserAccount { get; }
    string? IpAddress { get; }
    string? Module { get; }
    string? Reason { get; }
}
