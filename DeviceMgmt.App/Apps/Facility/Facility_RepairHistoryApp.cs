using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_RepairHistoryApp : BaseApp<v_Facility_RepairHistory>
{
    public Facility_RepairHistoryApp(IUnitWork unitWork, IRepository<v_Facility_RepairHistory> repository) : base(unitWork, repository)
    {
    }
}