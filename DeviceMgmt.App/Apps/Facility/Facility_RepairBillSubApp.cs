using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_RepairBillSubApp : BaseApp<Facility_RepairBillSub>
{
    public Facility_RepairBillSubApp(IUnitWork unitWork, IRepository<Facility_RepairBillSub> repository) : base(unitWork, repository)
    {
    }
}