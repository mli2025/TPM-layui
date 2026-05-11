using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("v_FacilityDD")]
public class v_FacilityDD : Entity
{
    public long ResourceId { get; set; }
    public string FacilityCode { get; set; } = string.Empty;
    public string FacilityName { get; set; } = string.Empty;
    public DateTime BillDate { get; set; } = DateTime.Now;
}
