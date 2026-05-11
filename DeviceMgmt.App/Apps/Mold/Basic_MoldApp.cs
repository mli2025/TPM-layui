using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Basic_MoldApp : BaseApp<Basic_Mold>
{
    public Basic_MoldApp(IUnitWork unitWork, IRepository<Basic_Mold> repository) : base(unitWork, repository)
    {
    }
}