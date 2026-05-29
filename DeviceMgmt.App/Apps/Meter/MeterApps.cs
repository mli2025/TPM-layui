using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Meter;

public class MeterApp : BaseApp<Repository.Domain.Meter>
{
    public MeterApp(IUnitWork unitWork, IRepository<Repository.Domain.Meter> repository)
        : base(unitWork, repository) { }

    public long Save(Repository.Domain.Meter e)
    {
        e.MeterCode = (e.MeterCode ?? string.Empty).Trim();
        var dup = Repository.Count("[MeterCode]=@c AND [Id]<>@id", new { c = e.MeterCode, id = e.Id });
        if (dup > 0) throw new InvalidOperationException("器具编号已存在");
        if (e.Id == 0) { if (e.Status == 0) e.Status = 1; Repository.Insert(e); }
        else Repository.Update(e);
        return e.Id;
    }
}

public class Meter_CalibPlanApp : BaseApp<Meter_CalibPlan>
{
    public Meter_CalibPlanApp(IUnitWork unitWork, IRepository<Meter_CalibPlan> repository)
        : base(unitWork, repository) { }
}

public class Meter_CalibRecordApp : BaseApp<Meter_CalibRecord>
{
    public Meter_CalibRecordApp(IUnitWork unitWork, IRepository<Meter_CalibRecord> repository)
        : base(unitWork, repository) { }

    /// <summary>复核确认后生效（GMP：复核通过方可生效）</summary>
    public void Review(long id, string reviewer)
        => Repository.ExecuteSql(
            "UPDATE [Meter_CalibRecord] SET [IsEffective]=1,[Reviewer]=@r,[ReviewDate]=getdate() WHERE [Id]=@id",
            new { id, r = reviewer });
}

public class Meter_SendOutApp : BaseApp<Meter_SendOut>
{
    private readonly IRepository<Meter_SendOutSub> _subRepo;

    public Meter_SendOutApp(IUnitWork unitWork, IRepository<Meter_SendOut> repository, IRepository<Meter_SendOutSub> subRepo)
        : base(unitWork, repository)
    {
        _subRepo = subRepo;
    }

    public long[] GetSubMeterIds(long mainId)
        => _subRepo.Find("[MainId]=@m", new { m = mainId }).Select(x => x.MeterId).ToArray();

    public void SetSubMeters(long mainId, long[] meterIds)
    {
        var existed = _subRepo.Find("[MainId]=@m", new { m = mainId }).ToList();
        var existedIds = existed.Select(x => x.MeterId).ToHashSet();
        var newIds = new HashSet<long>(meterIds ?? Array.Empty<long>());
        var toRemove = existed.Where(x => !newIds.Contains(x.MeterId)).Select(x => x.Id).ToArray();
        if (toRemove.Length > 0) _subRepo.Delete(toRemove);
        foreach (var mid in newIds.Where(m => !existedIds.Contains(m)))
            _subRepo.Insert(new Meter_SendOutSub { MainId = mainId, MeterId = mid });
    }

    public long SaveMain(Meter_SendOut m)
    {
        if (m.Id == 0) Repository.Insert(m); else Repository.Update(m);
        return m.Id;
    }

    public void DeleteCascade(long mainId)
    {
        SetSubMeters(mainId, Array.Empty<long>());
        Repository.Delete(mainId);
    }
}
