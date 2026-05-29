using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

/// <summary>登录日志（只读查询）</summary>
public class LoginLogApp : BaseApp<Sys_LoginLog>
{
    public LoginLogApp(IUnitWork unitWork, IRepository<Sys_LoginLog> repository)
        : base(unitWork, repository) { }
}

/// <summary>账户锁定：列表 + 管理员解锁</summary>
public class AccountLockApp : BaseApp<Sys_AccountLock>
{
    public AccountLockApp(IUnitWork unitWork, IRepository<Sys_AccountLock> repository)
        : base(unitWork, repository) { }

    public void Unlock(long id, string operatorName)
        => Repository.ExecuteSql(
            "UPDATE [Sys_AccountLock] SET [IsLocked]=0,[FailCount]=0,[UnlockedAt]=getdate(),[UnlockedBy]=@by WHERE [Id]=@id",
            new { id, by = operatorName });
}
