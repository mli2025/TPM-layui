using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_BillMainApp : BaseApp<Mold_BillMain>
{
    public Mold_BillMainApp(IUnitWork unitWork, IRepository<Mold_BillMain> repository) : base(unitWork, repository)
    {
    }
}