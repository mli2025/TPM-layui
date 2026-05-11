using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("v_Facility_ResourceDetailStatus")]
public class v_Facility_ResourceDetailStatus : Entity
{
    public int Status { get; set; }
    public DateTime? LastSpotCheck { get; set; }
}
