using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_OnOffSubApp : BaseApp<Mold_OnOffSub>
{
    public Mold_OnOffSubApp(IUnitWork unitWork, IRepository<Mold_OnOffSub> repository) : base(unitWork, repository)
    {
    }
}