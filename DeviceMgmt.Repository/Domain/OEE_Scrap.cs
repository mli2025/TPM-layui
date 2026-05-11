using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("OEE_Scrap")]
public class OEE_Scrap : Entity
{
    public int DeptId { get; set; }
    public int ResourceId { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Now;
    public int ClassId { get; set; }
    public int TaskBillId { get; set; }
    public int ScrapReasonId { get; set; }
    public int ScrapQty { get; set; }
}
