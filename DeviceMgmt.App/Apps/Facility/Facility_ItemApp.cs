using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_ItemApp : BaseApp<Facility_Item>
{
    public Facility_ItemApp(IUnitWork unitWork, IRepository<Facility_Item> repository) : base(unitWork, repository)
    {
    }
}