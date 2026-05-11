using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Basic_EquipmentResourcesApp : BaseApp<Basic_EquipmentResources>
{
    public Basic_EquipmentResourcesApp(IUnitWork unitWork, IRepository<Basic_EquipmentResources> repository) : base(unitWork, repository)
    {
    }
}