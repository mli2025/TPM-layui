using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

/// <summary>审计追踪（字段级变更明细）查询</summary>
public class AuditTrailApp : BaseApp<Sys_AuditTrail>
{
    public AuditTrailApp(IUnitWork unitWork, IRepository<Sys_AuditTrail> repository)
        : base(unitWork, repository) { }

    /// <summary>某条业务记录的全部变更时间线（按时间倒序）</summary>
    public List<Sys_AuditTrail> GetTimeline(string targetType, string targetId)
        => Repository.Find("[TargetType]=@t AND [TargetId]=@i", new { t = targetType, i = targetId }, "[CreateDate] DESC,[Id] DESC").ToList();
}
