using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("v_MoldQtyAlarm")]
public class v_MoldQtyAlarm : Entity
{
    public int? MaxUseQty { get; set; }
    public int? AlarmQty { get; set; }
    public int? NowUseQty { get; set; }
    public int? LastUseQty { get; set; }
    public int AlarmQtyFlag { get; set; }
}
