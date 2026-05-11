using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Basic;

public class EmployeeApp : BaseApp<Basic_Employee>
{
    public EmployeeApp(IUnitWork unitWork, IRepository<Basic_Employee> repository) : base(unitWork, repository)
    {
    }
}
