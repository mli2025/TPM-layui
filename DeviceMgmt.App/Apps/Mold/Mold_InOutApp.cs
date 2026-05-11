using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_InOutApp : BaseApp<Mold_InOut>
{
    public Mold_InOutApp(IUnitWork unitWork, IRepository<Mold_InOut> repository) : base(unitWork, repository)
    {
    }
}