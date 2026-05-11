using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Mold_RepairBill")]
public class Mold_RepairBill : Entity
{
    public string BillNo { get; set; } = string.Empty;
    public DateTime BillDate { get; set; } = DateTime.Now;
    public long MoldId { get; set; }
    public int Status { get; set; }
    public string? Descr { get; set; }
    public string? Remark { get; set; }
    public DateTime? LastRepairEnd { get; set; }
    public string? RepairStaff { get; set; }
    public DateTime? RepairBeginDate { get; set; }
    public DateTime? RepairEndDate { get; set; }
}
