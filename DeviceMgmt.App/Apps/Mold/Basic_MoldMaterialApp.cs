using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Basic_MoldMaterialApp : BaseApp<Basic_MoldMaterial>
{
    public Basic_MoldMaterialApp(IUnitWork unitWork, IRepository<Basic_MoldMaterial> repository) : base(unitWork, repository)
    {
    }
}