using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class OEE_TotalTimesApp : BaseApp<OEE_TotalTimes>
{
    public OEE_TotalTimesApp(IUnitWork unitWork, IRepository<OEE_TotalTimes> repository) : base(unitWork, repository)
    {
    }
}