using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Mold_OnOff")]
public class Mold_OnOff : Entity
{
    public long MoldId { get; set; }
    public long? OnPersonId { get; set; }
    public DateTime? OnDate { get; set; }
    public long? OffPersonId { get; set; }
    public DateTime? OffDate { get; set; }
    public long? ResourceId { get; set; }
    public short? Status { get; set; }
    public string? Remark { get; set; }
    public decimal UseQty { get; set; }
}
