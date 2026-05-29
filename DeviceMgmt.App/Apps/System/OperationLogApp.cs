using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

/// <summary>
/// 审计日志查询（URS 301-306）。日志只追加，界面只读，不提供修改/删除。
/// </summary>
public class OperationLogApp : BaseApp<Sys_OperationLog>
{
    public OperationLogApp(IUnitWork unitWork, IRepository<Sys_OperationLog> repository)
        : base(unitWork, repository)
    {
    }
}
