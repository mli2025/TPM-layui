using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Spare_InvoiceData")]
public class Spare_InvoiceData : Entity
{
    public int? SpareId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Specs { get; set; }
    public string? SafeStock { get; set; }
    public string? Remark { get; set; }
    public string? Status { get; set; }
    public string? Leibie { get; set; }
    public decimal? Danjia { get; set; }
    public string? Kehu { get; set; }
    public string? Danwei { get; set; }
    public decimal? QCQty { get; set; }
    public decimal? QCJe { get; set; }
    public decimal? InQty { get; set; }
    public decimal? InJe { get; set; }
    public decimal? OutQty { get; set; }
    public decimal? OutJe { get; set; }
    public decimal? JCQty { get; set; }
    public decimal? JCJe { get; set; }
}
