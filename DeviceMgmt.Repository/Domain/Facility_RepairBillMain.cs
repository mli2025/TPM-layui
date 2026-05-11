using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_RepairBillMain")]
public class Facility_RepairBillMain : Entity
{
    public string? BillNo { get; set; }
    public DateTime? BillDate { get; set; }
    public long? FacilityId { get; set; }
    public string? Descr { get; set; }
    public int? RepairTime { get; set; }
    public int? Status { get; set; }
    public string? Remark { get; set; }
    public DateTime? LastRepairEnd { get; set; }
    public string? Dispatch { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? RepairStaff { get; set; }
    public DateTime? RepairBeginDate { get; set; }
    public DateTime? RepairEndDate { get; set; }
    public string? Checker { get; set; }
    public DateTime? CheckDate { get; set; }
    public string? Closer { get; set; }
    public DateTime? CloseDate { get; set; }
    public string? Maker { get; set; }
    public DateTime? ResponseDate { get; set; }
    public int? OutsourcingFlag { get; set; }
    public DateTime? OutsourcingCreateDate { get; set; }
    public DateTime? OutsourcingLastDate { get; set; }
    public string? FaultCategory { get; set; }
    public string? FaultLocation { get; set; }
    public string? ProduceComfirm { get; set; }
    public string? EquipmentComfirm { get; set; }
    public string? QualityComfirm { get; set; }
    public int ComfirmFlag { get; set; }
    public DateTime? ProduceComfirmTime { get; set; }
    public DateTime? EquipmentComfirmTime { get; set; }
    public DateTime? QualityComfirmTime { get; set; }
    public long ReviewerUserId { get; set; }
    public DateTime? ReviewDateTime { get; set; }
    public string? ReviewRemark { get; set; }
    public string? FGC_Creator { get; set; }
    public string? FGC_CreateDate { get; set; }
    public string? FGC_LastModifier { get; set; }
    public string? FGC_LastModifyDate { get; set; }
}
