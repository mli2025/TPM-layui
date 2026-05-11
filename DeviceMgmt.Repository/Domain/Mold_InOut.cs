using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Mold_InOut")]
public class Mold_InOut : Entity
{
    public long MoldId { get; set; }
    public short Status { get; set; }
    public string Remark { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public string Type { get; set; } = string.Empty;
    public long PersonId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public long CreateUserld { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public long LastUpdateUserld { get; set; }
    public DateTime LastUpdateDate { get; set; } = DateTime.Now;
    public long AreaId { get; set; }
}
