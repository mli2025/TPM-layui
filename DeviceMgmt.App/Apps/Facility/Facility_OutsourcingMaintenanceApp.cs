using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_OutsourcingMaintenanceApp : BaseApp<Facility_OutsourcingMaintenance>
{
    public Facility_OutsourcingMaintenanceApp(IUnitWork unitWork, IRepository<Facility_OutsourcingMaintenance> repository) : base(unitWork, repository)
    {
    }
}