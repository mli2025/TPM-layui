using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class Spare_NowQuanApp : BaseApp<Spare_NowQuan>
{
    public Spare_NowQuanApp(IUnitWork unitWork, IRepository<Spare_NowQuan> repository) : base(unitWork, repository)
    {
    }
}