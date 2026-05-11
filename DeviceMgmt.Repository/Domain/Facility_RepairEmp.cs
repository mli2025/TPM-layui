using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_RepairEmp")]
public class Facility_RepairEmp : Entity
{
    public long WorkCenterId { get; set; }
    public long EmpId { get; set; }
    public long EmpId1 { get; set; }
    public long EmpId2 { get; set; }
    public long EmpId3 { get; set; }
    public int Time0 { get; set; }
    public int Time1 { get; set; }
    public int Status { get; set; }
    public long CreateUserId { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public long LastUpdateUserId { get; set; }
    public DateTime LastUpdateDate { get; set; } = DateTime.Now;
    public long BYSHUserId { get; set; }
}
