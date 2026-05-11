using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_TheTemplateMain")]
public class Facility_TheTemplateMain : Entity
{
    public string? HNumber { get; set; }
    public string? HName { get; set; }
    public string? Maker { get; set; }
    public string? Checker { get; set; }
    public DateTime? CheckDate { get; set; }
    public string? CloseMan { get; set; }
    public DateTime? CloseDate { get; set; }
    public DateTime? Hdate { get; set; }
    public short? Status { get; set; }
    public short Type { get; set; }
    public int? OutsourcingFlag { get; set; }
    public string? MaintenanceType { get; set; }
    public int? AlertDays { get; set; }
    public string? Files { get; set; }
    public string? FGC_Creator { get; set; }
    public string? FGC_CreateDate { get; set; }
    public string? FGC_LastModifier { get; set; }
    public string? FGC_LastModifyDate { get; set; }
}
