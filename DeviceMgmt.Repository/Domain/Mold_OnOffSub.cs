using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Mold_OnOffSub")]
public class Mold_OnOffSub : Entity
{
    public long MainId { get; set; }
    public long TaskBillId { get; set; }
    public long PlanId { get; set; }
    public long ReportId { get; set; }
    public string BarcodeCP { get; set; } = string.Empty;
    public long MoldId { get; set; }
    public decimal Qty { get; set; }
    public int Status { get; set; }
    public long CreateUserId { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public long LastUpdateUserId { get; set; }
    public DateTime LastUpdateDate { get; set; } = DateTime.Now;
    public decimal UpReportQty { get; set; }
    public decimal DropReportQty { get; set; }
}
