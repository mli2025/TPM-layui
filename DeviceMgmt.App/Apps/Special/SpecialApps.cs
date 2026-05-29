using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Special;

public class Special_EquipmentApp : BaseApp<Special_Equipment>
{
    public Special_EquipmentApp(IUnitWork unitWork, IRepository<Special_Equipment> repository)
        : base(unitWork, repository) { }

    public long Save(Special_Equipment e)
    {
        e.EquipCode = (e.EquipCode ?? string.Empty).Trim();
        var dup = Repository.Count("[EquipCode]=@c AND [Id]<>@id", new { c = e.EquipCode, id = e.Id });
        if (dup > 0) throw new InvalidOperationException("设备代码已存在");
        if (e.Id == 0) { if (e.Status == 0) e.Status = 1; Repository.Insert(e); }
        else Repository.Update(e);
        return e.Id;
    }
}

public class Special_InspectPlanApp : BaseApp<Special_InspectPlan>
{
    public Special_InspectPlanApp(IUnitWork unitWork, IRepository<Special_InspectPlan> repository)
        : base(unitWork, repository) { }
}

public class Special_InspectRecordApp : BaseApp<Special_InspectRecord>
{
    public Special_InspectRecordApp(IUnitWork unitWork, IRepository<Special_InspectRecord> repository)
        : base(unitWork, repository) { }
}
