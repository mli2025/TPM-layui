using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Mold_Item")]
public class Mold_Item : Entity
{
    public short Type { get; set; }
    public string? Project { get; set; }
    public string? CheckMethod { get; set; }
    public string? UpkeepMethod { get; set; }
    public string? Remark { get; set; }
    public short? Status { get; set; }
    public int? MoldType { get; set; }
    public int ControlType { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? StdMaxValue { get; set; }
    public decimal? StdMinValue { get; set; }
}
