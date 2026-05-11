using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Mold_BillMain")]
public class Mold_BillMain : Entity
{
    public string? BillNo { get; set; }
    public DateTime? BillDate { get; set; }
    public string? BillType { get; set; }
    public DateTime? BeginDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long? MoldID { get; set; }
    public long? TempID { get; set; }
    public string? MaintainType { get; set; }
    public int? Status { get; set; }
    public string? Remark { get; set; }
    public DateTime? LastMaintainTime { get; set; }
    public string? Dispatch { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? RepairStaff { get; set; }
    public DateTime? RepairStaffDate { get; set; }
    public string? Checker { get; set; }
    public DateTime? CheckDate { get; set; }
    public string? Closer { get; set; }
    public DateTime? CloseDate { get; set; }
    public long QXYId { get; set; }
    public long CreateUserld { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public long LastUpdateUserld { get; set; }
    public DateTime LastUpdateDate { get; set; } = DateTime.Now;
    public string? FGC_Creator { get; set; }
    public string? FGC_CreateDate { get; set; }
    public string? FGC_LastModifier { get; set; }
    public string? FGC_LastModifyDate { get; set; }
}
