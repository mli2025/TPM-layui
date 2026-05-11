using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_ResourceDetailGather")]
public class Facility_ResourceDetailGather : Entity
{
    public long FacilityId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public short Status { get; set; }
    public long CreateUserId { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public long LastUpdateUserId { get; set; }
    public DateTime LastUpdateDate { get; set; } = DateTime.Now;
}
