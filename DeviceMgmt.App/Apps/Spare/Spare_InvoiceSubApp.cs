using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class Spare_InvoiceSubApp : BaseApp<Spare_InvoiceSub>
{
    public Spare_InvoiceSubApp(IUnitWork unitWork, IRepository<Spare_InvoiceSub> repository) : base(unitWork, repository)
    {
    }
}