using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>
/// 系统版本发布记录
/// </summary>
[Table("Sys_Version")]
public class Sys_Version : Entity
{
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; } = DateTime.Now;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public bool IsCurrent { get; set; }
    public string? Author { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
