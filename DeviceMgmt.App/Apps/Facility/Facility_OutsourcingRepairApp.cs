using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_OutsourcingRepairApp : BaseApp<Facility_OutsourcingRepair>
{
    public Facility_OutsourcingRepairApp(IUnitWork unitWork, IRepository<Facility_OutsourcingRepair> repository) : base(unitWork, repository)
    {
    }
}