using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>
/// 用户组（URS 408：基于用户组的权限管理，权限叠加并集）
/// </summary>
[Table("Sys_UserGroup")]
public class Sys_UserGroup : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Descr { get; set; }
    public int Status { get; set; } = 1;
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

[Table("Sys_UserGroupUser")]
public class Sys_UserGroupUser : Entity
{
    public long GroupId { get; set; }
    public long UserId { get; set; }
}

[Table("Sys_UserGroupModule")]
public class Sys_UserGroupModule : Entity
{
    public long GroupId { get; set; }
    public long ModuleId { get; set; }
}
