using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class Basic_SpareApp : BaseApp<Basic_Spare>
{
    public Basic_SpareApp(IUnitWork unitWork, IRepository<Basic_Spare> repository) : base(unitWork, repository)
    {
    }
}