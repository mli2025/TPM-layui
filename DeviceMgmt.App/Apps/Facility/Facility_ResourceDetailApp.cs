using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_ResourceDetailApp : BaseApp<Facility_ResourceDetail>
{
    public Facility_ResourceDetailApp(IUnitWork unitWork, IRepository<Facility_ResourceDetail> repository) : base(unitWork, repository)
    {
    }
}