using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Spare_InvoiceSub")]
public class Spare_InvoiceSub : Entity
{
    public long? MainId { get; set; }
    public int? RowNum { get; set; }
    public long? SpareId { get; set; }
    public decimal? Qty { get; set; }
    public string? Remark { get; set; }
    public int? Status { get; set; }
    public decimal? RelationQty { get; set; }
    public decimal? NotRelationQty { get; set; }
    public decimal? Minpackage { get; set; }
    public string? Jinshouren { get; set; }
    public string? Danwei { get; set; }
    public decimal? Danjia { get; set; }
    public string? Kehu { get; set; }
    public decimal? Xindanjia { get; set; }
    public string? Bumen { get; set; }
    public string? jine { get; set; }
}
