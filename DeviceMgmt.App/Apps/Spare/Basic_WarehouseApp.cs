using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

public class Basic_WarehouseApp : BaseApp<Basic_Warehouse>
{
    public Basic_WarehouseApp(IUnitWork unitWork, IRepository<Basic_Warehouse> repository) : base(unitWork, repository)
    {
    }

    /// <summary>新增/编辑仓库主数据</summary>
    public long Save(Basic_Warehouse m)
    {
        m.Code = (m.Code ?? string.Empty).Trim();
        m.Name = (m.Name ?? string.Empty).Trim();
        if (m.Status == null) m.Status = 1;
        if (m.Id == 0) Repository.Insert(m);
        else Repository.Update(m);
        return m.Id;
    }
}
