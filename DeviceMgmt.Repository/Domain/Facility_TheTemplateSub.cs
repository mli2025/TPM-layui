using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_TheTemplateSub")]
public class Facility_TheTemplateSub : Entity
{
    public long HInspectionItemID { get; set; }
    public string? HRemark { get; set; }
    public int? ControlType { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? StdMaxValue { get; set; }
    public decimal? StdMinValue { get; set; }
    public long MainId { get; set; }
    public string HContent { get; set; } = string.Empty;
    public string HMethods { get; set; } = string.Empty;
    public string HStandard { get; set; } = string.Empty;
    public int? Maintenance_level { get; set; }
}
