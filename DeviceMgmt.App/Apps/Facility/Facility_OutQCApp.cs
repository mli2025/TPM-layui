using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Facility;

public class Facility_OutQCApp : BaseApp<Facility_OutQC>
{
    public Facility_OutQCApp(IUnitWork unitWork, IRepository<Facility_OutQC> repository) : base(unitWork, repository)
    {
    }
}