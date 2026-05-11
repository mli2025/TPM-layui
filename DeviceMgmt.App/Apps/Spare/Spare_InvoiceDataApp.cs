using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class Spare_InvoiceDataApp : BaseApp<Spare_InvoiceData>
{
    public Spare_InvoiceDataApp(IUnitWork unitWork, IRepository<Spare_InvoiceData> repository) : base(unitWork, repository)
    {
    }
}