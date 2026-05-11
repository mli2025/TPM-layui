using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_Status_HistoryApp : BaseApp<Facility_Status_History>
{
    public Facility_Status_HistoryApp(IUnitWork unitWork, IRepository<Facility_Status_History> repository) : base(unitWork, repository)
    {
    }
}