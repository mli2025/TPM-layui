using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_BillMainViewApp : BaseApp<v_Mold_BillMain>
{
    public Mold_BillMainViewApp(IUnitWork unitWork, IRepository<v_Mold_BillMain> repository) : base(unitWork, repository)
    {
    }
}