using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class DianJianDeptApp : BaseApp<v_dianjiandept>
{
    public DianJianDeptApp(IUnitWork unitWork, IRepository<v_dianjiandept> repository) : base(unitWork, repository)
    {
    }
}