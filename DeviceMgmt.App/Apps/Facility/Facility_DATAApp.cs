using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_DATAApp : BaseApp<Facility_DATA>
{
    public Facility_DATAApp(IUnitWork unitWork, IRepository<Facility_DATA> repository) : base(unitWork, repository)
    {
    }
}