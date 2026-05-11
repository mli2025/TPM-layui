using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Spare_InvoiceMain")]
public class Spare_InvoiceMain : Entity
{
    public string? BillNo { get; set; }
    public DateTime? BillDate { get; set; }
    public long? BillType { get; set; }
    public long? WHID { get; set; }
    public long? DeptId { get; set; }
    public long? toWHID { get; set; }
    public string? Remark { get; set; }
    public int? Status { get; set; }
    public string? Checker { get; set; }
    public DateTime? CheckDate { get; set; }
    public string? Closer { get; set; }
    public DateTime? CloseDate { get; set; }
    public long? Renyuan { get; set; }
    public string? Type { get; set; }
    public long? BillId { get; set; }
    public string? FGC_Creator { get; set; }
    public string? FGC_CreateDate { get; set; }
    public string? FGC_LastModifier { get; set; }
    public string? FGC_LastModifyDate { get; set; }
}
