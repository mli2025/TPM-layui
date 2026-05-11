using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_BillMainViewApp : BaseApp<v_Facility_BillMain>
{
    public Facility_BillMainViewApp(IUnitWork unitWork, IRepository<v_Facility_BillMain> repository) : base(unitWork, repository)
    {
    }
}