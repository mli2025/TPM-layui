using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("OEE_Rate")]
public class OEE_Rate : Entity
{
    public int DeptId { get; set; }
    public int ResourceId { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Now;
    public int ClassId { get; set; }
    public int TaskBillId { get; set; }
    public int MaterialId { get; set; }
    public decimal stdTimes { get; set; }
    public int ReportQty { get; set; }
    public TimeSpan StopStart { get; set; }
    public TimeSpan StopEnd { get; set; }
}
