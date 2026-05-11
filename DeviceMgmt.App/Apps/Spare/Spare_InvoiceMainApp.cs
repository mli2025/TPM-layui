using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class Spare_InvoiceMainApp : BaseApp<Spare_InvoiceMain>
{
    public Spare_InvoiceMainApp(IUnitWork unitWork, IRepository<Spare_InvoiceMain> repository) : base(unitWork, repository)
    {
    }
}