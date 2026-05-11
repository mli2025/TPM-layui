using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class OEE_ScrapApp : BaseApp<OEE_Scrap>
{
    public OEE_ScrapApp(IUnitWork unitWork, IRepository<OEE_Scrap> repository) : base(unitWork, repository)
    {
    }
}