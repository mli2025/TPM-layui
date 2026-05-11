using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Sys_Role")]
public class Sys_Role : Entity
{
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
}

[Table("Sys_UserRole")]
public class Sys_UserRole : Entity
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
}
