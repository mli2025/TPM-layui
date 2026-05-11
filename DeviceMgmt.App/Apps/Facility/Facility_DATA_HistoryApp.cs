using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_DATA_HistoryApp : BaseApp<Facility_DATA_History>
{
    public Facility_DATA_HistoryApp(IUnitWork unitWork, IRepository<Facility_DATA_History> repository) : base(unitWork, repository)
    {
    }
}