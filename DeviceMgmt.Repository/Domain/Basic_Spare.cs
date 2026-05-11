using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Basic_Spare")]
public class Basic_Spare : Entity
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Specs { get; set; }
    public decimal? SafeStock { get; set; }
    public int? Remark { get; set; }
    public int? Status { get; set; }
    public string? Leibie { get; set; }
    public decimal? Danjia { get; set; }
    public string? Kehu { get; set; }
    public string? Danwei { get; set; }
}
