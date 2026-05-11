using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_TheTemplateSubApp : BaseApp<Facility_TheTemplateSub>
{
    public Facility_TheTemplateSubApp(IUnitWork unitWork, IRepository<Facility_TheTemplateSub> repository) : base(unitWork, repository)
    {
    }
}