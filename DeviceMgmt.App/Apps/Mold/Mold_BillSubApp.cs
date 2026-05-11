using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_BillSubApp : BaseApp<Mold_BillSub>
{
    public Mold_BillSubApp(IUnitWork unitWork, IRepository<Mold_BillSub> repository) : base(unitWork, repository)
    {
    }
}