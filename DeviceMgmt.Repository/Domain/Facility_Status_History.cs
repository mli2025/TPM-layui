using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_Status_History")]
public class Facility_Status_History : Entity
{
    public int HResourceId { get; set; }
    public string HOPStr { get; set; } = string.Empty;
    public DateTime HWorkDate { get; set; } = DateTime.Now;
    public string HOperator { get; set; } = string.Empty;
}
