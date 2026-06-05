using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_BillMain")]
public class Facility_BillMain : Entity
{
    public string? BillNo { get; set; }
    public DateTime? BillDate { get; set; }
    public string? BillType { get; set; }
    public DateTime? BeginDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ChangedBeginDate { get; set; }
    public DateTime? ChangedEndDate { get; set; }
    public long? FacilityID { get; set; }
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
    public int? Maintenance_level { get; set; }
    public int? IsOK { get; set; }
    public decimal Amount { get; set; }
    public string? Files { get; set; }
    public long CreateUserId { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public long LastUpdateUserId { get; set; }
    public DateTime LastUpdateDate { get; set; } = DateTime.Now;
    public long CheckerUserId { get; set; }
    public string? FGC_Creator { get; set; }
    public string? FGC_CreateDate { get; set; }
    public string? FGC_LastModifier { get; set; }
    public string? FGC_LastModifyDate { get; set; }
}
