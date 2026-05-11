using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("OEE_TotalTimes")]
public class OEE_TotalTimes : Entity
{
    public int DeptId { get; set; }
    public int ResourceId { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Now;
    public int ClassId { get; set; }
    public decimal TotalTimes { get; set; }
    public decimal NotHavTaskTimes { get; set; }
}
