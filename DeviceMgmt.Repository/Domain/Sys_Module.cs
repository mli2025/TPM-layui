using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Sys_Module")]
public class Sys_Module : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Url { get; set; }
    public long ParentId { get; set; }
    public int Sort { get; set; }
    public int Status { get; set; }
    public string? Icon { get; set; }
}

[Table("Sys_ModuleButtons")]
public class Sys_ModuleButtons : Entity
{
    public long ModuleId { get; set; }
    public string DomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

[Table("Sys_RoleModule")]
public class Sys_RoleModule : Entity
{
    public long RoleId { get; set; }
    public long ModuleId { get; set; }
}
