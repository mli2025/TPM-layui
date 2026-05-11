using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_DayAlarmApp : BaseApp<v_MoldDayAlarm>
{
    public Mold_DayAlarmApp(IUnitWork unitWork, IRepository<v_MoldDayAlarm> repository) : base(unitWork, repository)
    {
    }
}