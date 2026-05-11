using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_TheTemplateMainApp : BaseApp<Mold_TheTemplateMain>
{
    public Mold_TheTemplateMainApp(IUnitWork unitWork, IRepository<Mold_TheTemplateMain> repository) : base(unitWork, repository)
    {
    }
}