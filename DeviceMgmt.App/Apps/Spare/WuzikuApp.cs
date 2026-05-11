using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class WuzikuApp : BaseApp<Wuziku>
{
    public WuzikuApp(IUnitWork unitWork, IRepository<Wuziku> repository) : base(unitWork, repository)
    {
    }
}