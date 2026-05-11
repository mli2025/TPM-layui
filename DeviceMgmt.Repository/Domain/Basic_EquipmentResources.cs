using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Basic_EquipmentResources")]
public class Basic_EquipmentResources : Entity
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Remark { get; set; }
    public short? Status { get; set; }
    public DateTime? EnterDate { get; set; }
    public int? DeptId { get; set; }
}
