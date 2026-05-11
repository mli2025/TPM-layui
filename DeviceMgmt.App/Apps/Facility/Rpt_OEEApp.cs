using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Rpt_OEEApp : BaseApp<rpt_OEE>
{
    public Rpt_OEEApp(IUnitWork unitWork, IRepository<rpt_OEE> repository) : base(unitWork, repository)
    {
    }
}