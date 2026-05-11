using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_ResourceDetailGatherApp : BaseApp<Facility_ResourceDetailGather>
{
    public Facility_ResourceDetailGatherApp(IUnitWork unitWork, IRepository<Facility_ResourceDetailGather> repository) : base(unitWork, repository)
    {
    }
}