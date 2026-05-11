using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_OutsourcingRepair")]
public class Facility_OutsourcingRepair : Entity
{
    public int RepairBillMainid { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? FaultDescription { get; set; }
    public int? Status { get; set; }
    public string? Creater { get; set; }
    public string? Acceptance { get; set; }
    public string? Appendix { get; set; }
    public DateTime? CreaterDate { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public string? AcceptanceComments { get; set; }
    public string? FaultLocation { get; set; }
    public string? FaultCategory { get; set; }
    public decimal Amount { get; set; }
}
