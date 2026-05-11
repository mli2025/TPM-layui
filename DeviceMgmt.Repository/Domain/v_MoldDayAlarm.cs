using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("v_MoldDayAlarm")]
public class v_MoldDayAlarm : Entity
{
    public int? MaxUseDay { get; set; }
    public int? AlarmDay { get; set; }
    public int? NowUseDay { get; set; }
    public int? LastUseQty { get; set; }
    public int AlarmDayFlag { get; set; }
}
