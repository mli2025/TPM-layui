using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_Item")]
public class Facility_Item : Entity
{
    public short Type { get; set; }
    public string? Project { get; set; }
    public string? CheckMethod { get; set; }
    public string? UpkeepMethod { get; set; }
    public string? Remark { get; set; }
    public short? Status { get; set; }
    public string FacilityType { get; set; } = string.Empty;
    public int ControlType { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? StdMaxValue { get; set; }
    public decimal? StdMinValue { get; set; }
    public int? Maintenance_level { get; set; }
    public decimal? Standardvalue { get; set; }
    public int WXFlage { get; set; }
}
