using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Basic_Mold")]
public class Basic_Mold : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? MoldType { get; set; }
    public string? ConnectFacilityType { get; set; }
    public long? WarehouseId { get; set; }
    public long? WareAreaId { get; set; }
    public string? WareArea { get; set; }
    public long? SupplierId { get; set; }
    public string? Remark { get; set; }
    public int? MaxUseQty { get; set; }
    public int? MaxUseDay { get; set; }
    public int? AlarmQty { get; set; }
    public int? AlarmDay { get; set; }
    public int? TotalUseQty { get; set; }
    public DateTime? LastRepairTime { get; set; }
    public long? LastRepairUserId { get; set; }
    public long? FacilityId { get; set; }
    public long? ResourceId { get; set; }
    public int? NowUseQty { get; set; }
    public int? Status { get; set; }
    public long? TheQtyTemplateMainId { get; set; }
    public long? TheDayTemplateMainId { get; set; }
    public int? QiangXueQty { get; set; }
    public string? Type { get; set; }
    public decimal? GWThickness { get; set; }
    public int? ThickenedFlag { get; set; }
    public decimal StockQty { get; set; }
    public decimal MoldQty { get; set; }
    public int SpotCheckFlag { get; set; }
    public int CleanFlag { get; set; }
    public int ResetFlag { get; set; }
    public int AllUseQty { get; set; }
    public int LastUseQty { get; set; }
}
