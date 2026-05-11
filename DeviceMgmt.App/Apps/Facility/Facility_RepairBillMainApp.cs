using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_RepairBillMainApp : BaseApp<Facility_RepairBillMain>
{
    public Facility_RepairBillMainApp(IUnitWork unitWork, IRepository<Facility_RepairBillMain> repository) : base(unitWork, repository)
    {
    }
}