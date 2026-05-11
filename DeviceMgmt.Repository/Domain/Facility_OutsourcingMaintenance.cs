using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_OutsourcingMaintenance")]
public class Facility_OutsourcingMaintenance : Entity
{
    public int BillMainid { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? MaintenanceType { get; set; }
    public int? Status { get; set; }
    public string? Creater { get; set; }
    public string? Acceptance { get; set; }
    public string? Appendix { get; set; }
    public DateTime? EstimatedTime { get; set; }
    public DateTime? CreaterTime { get; set; }
    public DateTime? AcceptanceTime { get; set; }
    public int? SupplierID { get; set; }
    public string? Maintainer { get; set; }
    public long FacilityId { get; set; }
    public long CreateUserId { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public long LastUpdateUserId { get; set; }
    public DateTime LastUpdateDate { get; set; } = DateTime.Now;
}
