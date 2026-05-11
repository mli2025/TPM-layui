using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Basic_Employee")]
public class Basic_Employee : Entity
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long DeptId { get; set; }
    public int Status { get; set; }
}
