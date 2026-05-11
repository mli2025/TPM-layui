using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

public class UserApp : BaseApp<Sys_User>
{
    public UserApp(IUnitWork unitWork, IRepository<Sys_User> repository) : base(unitWork, repository)
    {
    }

    public Sys_User? GetByAccount(string account)
        => Repository.FindSingle("[Account]=@a", new { a = account });
}
