using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_RepairEmpApp : BaseApp<Facility_RepairEmp>
{
    public Facility_RepairEmpApp(IUnitWork unitWork, IRepository<Facility_RepairEmp> repository) : base(unitWork, repository)
    {
    }
}