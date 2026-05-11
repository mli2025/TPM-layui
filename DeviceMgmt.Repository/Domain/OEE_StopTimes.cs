using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("OEE_StopTimes")]
public class OEE_StopTimes : Entity
{
    public int DeptId { get; set; }
    public int ResourceId { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Now;
    public int ClassId { get; set; }
    public int StopReasonId { get; set; }
    public TimeSpan StopStart { get; set; }
    public TimeSpan StopEnd { get; set; }
    public decimal StopTimes { get; set; }
}
