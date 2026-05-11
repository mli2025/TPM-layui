using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Basic_MoldMaterial")]
public class Basic_MoldMaterial : Entity
{
    public long MoldId { get; set; }
    public long MaterialId { get; set; }
    public decimal Qty { get; set; }
}
