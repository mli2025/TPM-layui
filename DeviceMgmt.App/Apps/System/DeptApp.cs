using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

public class DeptApp : BaseApp<Sys_Dept>
{
    public DeptApp(IUnitWork unitWork, IRepository<Sys_Dept> repository) : base(unitWork, repository)
    {
    }

    /// <summary>
    /// 保存部门。重复时抛出 <see cref="InvalidOperationException"/>（编码全局唯一；同上级下名称唯一）。
    /// </summary>
    public long Save(Sys_Dept d)
    {
        d.DeptName = (d.DeptName ?? string.Empty).Trim();
        d.DeptNumber = (d.DeptNumber ?? string.Empty).Trim();
        EnsureNotDuplicate(d);

        if (d.Id == 0)
        {
            if (d.Status == 0) d.Status = 1;
            Repository.Insert(d);
        }
        else
        {
            Repository.Update(d);
        }
        return d.Id;
    }

    private void EnsureNotDuplicate(Sys_Dept d)
    {
        var dupName = Repository.Count(
            "[ParentId]=@p AND [DeptName]=@name AND [Id]<>@id",
            new { p = d.ParentId, name = d.DeptName, id = d.Id });
        if (dupName > 0)
            throw new InvalidOperationException("同一上级下已存在相同名称的部门");

        if (string.IsNullOrEmpty(d.DeptNumber)) return;

        var dupNum = Repository.Count(
            "[DeptNumber]=@num AND [Id]<>@id",
            new { num = d.DeptNumber, id = d.Id });
        if (dupNum > 0)
            throw new InvalidOperationException("部门编号已被使用");
    }
}
