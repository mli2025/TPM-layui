using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_RepairBillSub")]
public class Facility_RepairBillSub : Entity
{
    public long? MainId { get; set; }
    public long? ReasonId { get; set; }
    public int? Sort { get; set; }
    public string? Remark { get; set; }
    public string? Descr { get; set; }
    public string? FaultAnalysis { get; set; }
    public string? PreventiveMeasure { get; set; }
}
