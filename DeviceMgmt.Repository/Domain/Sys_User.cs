using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Sys_User")]
public class Sys_User : Entity
{
    public string Account { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Name { get; set; }
    public long EmployeeId { get; set; }
    public long DeptId { get; set; }
    public int Status { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
