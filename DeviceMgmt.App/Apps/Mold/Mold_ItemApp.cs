using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_ItemApp : BaseApp<Mold_Item>
{
    public Mold_ItemApp(IUnitWork unitWork, IRepository<Mold_Item> repository) : base(unitWork, repository)
    {
    }
}