using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_TheTemplateMainApp : BaseApp<Facility_TheTemplateMain>
{
    public Facility_TheTemplateMainApp(IUnitWork unitWork, IRepository<Facility_TheTemplateMain> repository) : base(unitWork, repository)
    {
    }
}