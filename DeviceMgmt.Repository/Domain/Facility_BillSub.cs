using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_BillSub")]
public class Facility_BillSub : Entity
{
    public long MainId { get; set; }
    public string Project { get; set; } = string.Empty;
    public string CheckMethod { get; set; } = string.Empty;
    public string UpkeepMethod { get; set; } = string.Empty;
    public string? Result { get; set; }
    public int ControlType { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? StdMaxValue { get; set; }
    public decimal? StdMinValue { get; set; }
    public string? Remark { get; set; }
    public int WXFlage { get; set; }
}
