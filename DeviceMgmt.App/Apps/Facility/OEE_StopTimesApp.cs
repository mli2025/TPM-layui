using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class OEE_StopTimesApp : BaseApp<OEE_StopTimes>
{
    public OEE_StopTimesApp(IUnitWork unitWork, IRepository<OEE_StopTimes> repository) : base(unitWork, repository)
    {
    }
}