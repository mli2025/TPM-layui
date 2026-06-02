using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>仓库主数据（备件出入库 / 库存归属）</summary>
[Table("Basic_Warehouse")]
public class Basic_Warehouse : Entity
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Manager { get; set; }
    public string? Remark { get; set; }
    public int? Status { get; set; }
}
