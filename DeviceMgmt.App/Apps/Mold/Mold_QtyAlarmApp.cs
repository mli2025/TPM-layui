using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_QtyAlarmApp : BaseApp<v_MoldQtyAlarm>
{
    public Mold_QtyAlarmApp(IUnitWork unitWork, IRepository<v_MoldQtyAlarm> repository) : base(unitWork, repository)
    {
    }
}