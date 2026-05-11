using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_BillMainApp : BaseApp<Facility_BillMain>
{
    public Facility_BillMainApp(IUnitWork unitWork, IRepository<Facility_BillMain> repository) : base(unitWork, repository)
    {
    }
}