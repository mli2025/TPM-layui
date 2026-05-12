using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Infrastructure.DEncrypt;

namespace DeviceMgmt.App.Apps.System;

public class UserApp : BaseApp<Sys_User>
{
    public UserApp(IUnitWork unitWork, IRepository<Sys_User> repository) : base(unitWork, repository)
    {
    }

    public Sys_User? GetByAccount(string account)
        => Repository.FindSingle("[Account]=@a", new { a = account });

    public long SaveUser(Sys_User u, string? rawPassword)
    {
        if (u.Id == 0)
        {
            if (u.Status == 0) u.Status = 1;
            if (string.IsNullOrEmpty(rawPassword)) rawPassword = "123456";
            u.Password = DesEncrypt.Md5(rawPassword);
            Repository.Insert(u);
        }
        else
        {
            var old = Repository.FindSingle(u.Id);
            if (old == null) throw new InvalidOperationException("用户不存在");
            old.Account = u.Account;
            old.Name = u.Name;
            old.EmployeeId = u.EmployeeId;
            old.DeptId = u.DeptId;
            old.Status = u.Status;
            if (!string.IsNullOrEmpty(rawPassword))
            {
                old.Password = DesEncrypt.Md5(rawPassword);
            }
            Repository.Update(old);
        }
        return u.Id;
    }

    public void ResetPassword(long userId, string newPassword)
    {
        Repository.ExecuteSql("UPDATE [Sys_User] SET [Password]=@p WHERE [Id]=@id",
            new { p = DesEncrypt.Md5(newPassword), id = userId });
    }
}
