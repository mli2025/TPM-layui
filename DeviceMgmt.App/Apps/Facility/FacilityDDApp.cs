using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class FacilityDDApp : BaseApp<v_FacilityDD>
{
    public FacilityDDApp(IUnitWork unitWork, IRepository<v_FacilityDD> repository) : base(unitWork, repository)
    {
    }
}