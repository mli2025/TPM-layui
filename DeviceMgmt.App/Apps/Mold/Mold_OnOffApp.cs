using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_OnOffApp : BaseApp<Mold_OnOff>
{
    public Mold_OnOffApp(IUnitWork unitWork, IRepository<Mold_OnOff> repository) : base(unitWork, repository)
    {
    }
}