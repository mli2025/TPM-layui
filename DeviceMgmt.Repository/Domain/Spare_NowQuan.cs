using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Spare_NowQuan")]
public class Spare_NowQuan : Entity
{
    public long? SpareId { get; set; }
    public long? WarehouseId { get; set; }
    public long? AreaId { get; set; }
    public decimal? Qty { get; set; }
    public string? Danjiaid { get; set; }
}
