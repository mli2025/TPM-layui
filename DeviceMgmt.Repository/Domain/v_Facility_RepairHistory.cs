using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("v_Facility_RepairHistory")]
public class v_Facility_RepairHistory : Entity
{
    public string? BillNo { get; set; }
    public DateTime? RecordDate { get; set; }
    public string? EmpName { get; set; }
    public long? FacilityId { get; set; }
    public int Status { get; set; }
    public long? ReasonId { get; set; }
}
