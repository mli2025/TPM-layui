using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>
/// 系统级全局设置（按 Key 唯一）
/// </summary>
[Table("Sys_Setting")]
public class Sys_Setting : Entity
{
    public string Group { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string ValueType { get; set; } = "string";
    public string Title { get; set; } = string.Empty;
    public string? Descr { get; set; }
    public int Sort { get; set; }
    public bool Editable { get; set; } = true;
    public DateTime UpdateDate { get; set; } = DateTime.Now;
}
