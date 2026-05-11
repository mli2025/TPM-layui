using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class WMS_BarCodeInfo_SparesApp : BaseApp<WMS_BarCodeInfo_Spares>
{
    public WMS_BarCodeInfo_SparesApp(IUnitWork unitWork, IRepository<WMS_BarCodeInfo_Spares> repository) : base(unitWork, repository)
    {
    }
}