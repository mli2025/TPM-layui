using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

/// <summary>批量导入日志查询</summary>
public class ImportLogApp : BaseApp<Sys_ImportLog>
{
    public ImportLogApp(IUnitWork unitWork, IRepository<Sys_ImportLog> repository)
        : base(unitWork, repository) { }
}
