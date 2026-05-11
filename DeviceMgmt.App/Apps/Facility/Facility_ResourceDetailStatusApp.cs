using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_ResourceDetailStatusApp : BaseApp<v_Facility_ResourceDetailStatus>
{
    public Facility_ResourceDetailStatusApp(IUnitWork unitWork, IRepository<v_Facility_ResourceDetailStatus> repository) : base(unitWork, repository)
    {
    }
}