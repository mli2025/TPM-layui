using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Sys_Dept")]
public class Sys_Dept : Entity
{
    public string DeptNumber { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
    public long ParentId { get; set; }
    public int Status { get; set; }
}
