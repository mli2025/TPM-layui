using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_ProcessApp : BaseApp<Facility_Process>
{
    public Facility_ProcessApp(IUnitWork unitWork, IRepository<Facility_Process> repository) : base(unitWork, repository)
    {
    }
}