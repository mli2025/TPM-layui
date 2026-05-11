using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Production_BarcodeSMTApp : BaseApp<V_Production_BarcodeSMT>
{
    public Production_BarcodeSMTApp(IUnitWork unitWork, IRepository<V_Production_BarcodeSMT> repository) : base(unitWork, repository)
    {
    }
}