using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class OEE_RateApp : BaseApp<OEE_Rate>
{
    public OEE_RateApp(IUnitWork unitWork, IRepository<OEE_Rate> repository) : base(unitWork, repository)
    {
    }
}