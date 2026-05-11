using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_TheTemplateSubApp : BaseApp<Mold_TheTemplateSub>
{
    public Mold_TheTemplateSubApp(IUnitWork unitWork, IRepository<Mold_TheTemplateSub> repository) : base(unitWork, repository)
    {
    }
}