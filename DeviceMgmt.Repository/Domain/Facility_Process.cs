using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_Process")]
public class Facility_Process : Entity
{
    public long? FacilityId { get; set; }
    public string? Type { get; set; }
    public DateTime? Date { get; set; }
    public string? FDesc { get; set; }
}
