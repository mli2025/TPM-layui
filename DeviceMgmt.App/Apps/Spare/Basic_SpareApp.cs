using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class Basic_SpareApp : BaseApp<Basic_Spare>
{
    public Basic_SpareApp(IUnitWork unitWork, IRepository<Basic_Spare> repository) : base(unitWork, repository)
    {
    }

    /// <summary>新增/编辑备件主数据（URS 1201）</summary>
    public long Save(Basic_Spare m)
    {
        m.Code = (m.Code ?? string.Empty).Trim();
        if (m.Status == null) m.Status = 1;
        if (m.Id == 0) Repository.Insert(m);
        else Repository.Update(m);
        return m.Id;
    }
}