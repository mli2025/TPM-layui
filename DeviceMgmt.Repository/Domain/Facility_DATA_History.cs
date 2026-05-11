using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_DATA_History")]
public class Facility_DATA_History : Entity
{
    public int HResourceId { get; set; }
    public DateTime HWorkDate { get; set; } = DateTime.Now;
    public string HStatusStr { get; set; } = string.Empty;
    public int HWorkQty { get; set; }
    public int HStopTimes { get; set; }
}
