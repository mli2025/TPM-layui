using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Basic;

public class EmployeeApp : BaseApp<Basic_Employee>
{
    public EmployeeApp(IUnitWork unitWork, IRepository<Basic_Employee> repository) : base(unitWork, repository)
    {
    }

    /// <summary>新增/编辑：EmployeeNumber 全表唯一，Name 必填，DeptId 必填</summary>
    public long Save(Basic_Employee model)
    {
        if (model == null) throw new InvalidOperationException("数据为空");
        if (string.IsNullOrWhiteSpace(model.EmployeeNumber)) throw new InvalidOperationException("工号必填");
        if (string.IsNullOrWhiteSpace(model.Name)) throw new InvalidOperationException("姓名必填");
        if (model.DeptId <= 0) throw new InvalidOperationException("请选择所属部门");

        model.EmployeeNumber = model.EmployeeNumber.Trim();
        model.Name = model.Name.Trim();

        var duplicate = Repository.Count(
            "[EmployeeNumber]=@no AND [Id]<>@id",
            new { no = model.EmployeeNumber, id = model.Id });
        if (duplicate > 0) throw new InvalidOperationException($"工号 {model.EmployeeNumber} 已存在");

        if (model.Id == 0)
        {
            Add(model);
        }
        else
        {
            Update(model);
        }
        return model.Id;
    }
}
