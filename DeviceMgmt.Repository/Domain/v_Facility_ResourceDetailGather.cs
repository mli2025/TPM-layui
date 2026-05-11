using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("v_Facility_ResourceDetailGather")]
public class v_Facility_ResourceDetailGather : Entity
{
    public string FacilityCode { get; set; } = string.Empty;
    public string FacilityName { get; set; } = string.Empty;
    public long DeptId { get; set; }
    public short Status { get; set; }
}
