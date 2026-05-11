using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class WMS_BarCodeInfo_Spares_SubApp : BaseApp<WMS_BarCodeInfo_Spares_Sub>
{
    public WMS_BarCodeInfo_Spares_SubApp(IUnitWork unitWork, IRepository<WMS_BarCodeInfo_Spares_Sub> repository) : base(unitWork, repository)
    {
    }
}