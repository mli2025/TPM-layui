using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_ResourceDetailGatherViewApp : BaseApp<v_Facility_ResourceDetailGather>
{
    public Facility_ResourceDetailGatherViewApp(IUnitWork unitWork, IRepository<v_Facility_ResourceDetailGather> repository) : base(unitWork, repository)
    {
    }
}