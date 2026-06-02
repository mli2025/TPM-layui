using DeviceMgmt.App.Request;
using DeviceMgmt.App.Response;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Infrastructure.DEncrypt;

namespace DeviceMgmt.App.Apps.System;

public class UserApp : BaseApp<Sys_User>
{
    private readonly IRepository<Sys_Dept> _deptRepo;
    private readonly IRepository<Basic_Employee> _empRepo;

    public UserApp(
        IUnitWork unitWork,
        IRepository<Sys_User> repository,
        IRepository<Sys_Dept> deptRepo,
        IRepository<Basic_Employee> empRepo)
        : base(unitWork, repository)
    {
        _deptRepo = deptRepo;
        _empRepo = empRepo;
    }

    /// <summary>用户列表附带部门名称（Layui 表格展示用）。</summary>
    public override TableData Getmainlist(PageReq req, long? deptId = null)
    {
        var filters = GetSearchCondition(req.searchParam);
        var orderBy = BuildOrderBy(req.sfield, req.sorder);
        var (data, total) = Repository.FindPaged(filters, req.page, req.limit, orderBy);
        var list = data.ToList();
        var deptMap = _deptRepo.Find(null, null, "[Id] ASC").ToDictionary(d => d.Id, d => d.DeptName ?? string.Empty);
        var empMap = _empRepo.Find("[Status]=1", null, "[Id] ASC")
            .ToDictionary(e => e.Id, e => e.EmployeeNumber ?? string.Empty);
        var rows = list.Select(u => new
        {
            u.Id,
            u.Account,
            u.Name,
            u.EmployeeId,
            EmployeeNumber = u.EmployeeId != 0 && empMap.TryGetValue(u.EmployeeId, out var en) ? en : string.Empty,
            u.DeptId,
            DeptName = u.DeptId != 0 && deptMap.TryGetValue(u.DeptId, out var dn) ? dn : string.Empty,
            u.CreateDate,
            u.Status
        }).ToList();
        return new TableData { code = 0, count = total, data = rows };
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
