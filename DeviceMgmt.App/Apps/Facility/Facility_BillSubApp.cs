using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_BillSubApp : BaseApp<Facility_BillSub>
{
    public Facility_BillSubApp(IUnitWork unitWork, IRepository<Facility_BillSub> repository) : base(unitWork, repository)
    {
    }
}