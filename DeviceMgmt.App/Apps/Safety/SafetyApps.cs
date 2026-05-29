using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Safety;

public class Safety_AccessoryApp : BaseApp<Safety_Accessory>
{
    public Safety_AccessoryApp(IUnitWork unitWork, IRepository<Safety_Accessory> repository)
        : base(unitWork, repository) { }

    public long Save(Safety_Accessory e)
    {
        e.AccCode = (e.AccCode ?? string.Empty).Trim();
        var dup = Repository.Count("[AccCode]=@c AND [Id]<>@id", new { c = e.AccCode, id = e.Id });
        if (dup > 0) throw new InvalidOperationException("附件编号已存在");
        if (e.Id == 0) { if (e.Status == 0) e.Status = 1; Repository.Insert(e); }
        else Repository.Update(e);
        return e.Id;
    }
}

public class Safety_CheckPlanApp : BaseApp<Safety_CheckPlan>
{
    public Safety_CheckPlanApp(IUnitWork unitWork, IRepository<Safety_CheckPlan> repository)
        : base(unitWork, repository) { }
}

public class Safety_CheckRecordApp : BaseApp<Safety_CheckRecord>
{
    public Safety_CheckRecordApp(IUnitWork unitWork, IRepository<Safety_CheckRecord> repository)
        : base(unitWork, repository) { }
}
