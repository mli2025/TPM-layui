using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Mold;

public class Mold_RepairBillApp : BaseApp<Mold_RepairBill>
{
    public Mold_RepairBillApp(IUnitWork unitWork, IRepository<Mold_RepairBill> repository) : base(unitWork, repository)
    {
    }
}