using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Inspect;

/// <summary>点检标准（主子）</summary>
public class Inspect_StandardApp : BaseApp<Inspect_Standard>
{
    private readonly IRepository<Inspect_StandardSub> _subRepo;

    public Inspect_StandardApp(IUnitWork unitWork, IRepository<Inspect_Standard> repository, IRepository<Inspect_StandardSub> subRepo)
        : base(unitWork, repository) { _subRepo = subRepo; }

    public List<Inspect_StandardSub> GetSubs(long mainId)
        => _subRepo.Find("[MainId]=@m", new { m = mainId }, "[Sort] ASC,[Id] ASC").ToList();

    public long Save(Inspect_Standard main, IEnumerable<Inspect_StandardSub>? subs)
    {
        if (main.Id == 0) { main.CreateDate = DateTime.Now; if (main.Status == 0) main.Status = 1; Repository.Insert(main); }
        else Repository.Update(main);
        var existed = _subRepo.Find("[MainId]=@m", new { m = main.Id }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) _subRepo.Delete(existed);
        var sort = 1;
        foreach (var s in subs ?? Enumerable.Empty<Inspect_StandardSub>())
        {
            if (string.IsNullOrWhiteSpace(s.ItemName)) continue;
            s.Id = 0; s.MainId = main.Id; s.Sort = sort++;
            _subRepo.Insert(s);
        }
        return main.Id;
    }

    public void DeleteCascade(long id)
    {
        var subIds = _subRepo.Find("[MainId]=@m", new { m = id }).Select(x => x.Id).ToArray();
        if (subIds.Length > 0) _subRepo.Delete(subIds);
        Repository.Delete(id);
    }
}

/// <summary>点检计划</summary>
public class Inspect_PlanApp : BaseApp<Inspect_Plan>
{
    public Inspect_PlanApp(IUnitWork unitWork, IRepository<Inspect_Plan> repository) : base(unitWork, repository) { }

    public long SavePlan(Inspect_Plan m)
    {
        if (m.Id == 0)
        {
            m.CreateDate = DateTime.Now;
            if (string.IsNullOrWhiteSpace(m.PlanNo)) m.PlanNo = "IP" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Repository.Insert(m);
        }
        else Repository.Update(m);
        return m.Id;
    }

    /// <summary>某月计划（日历视图用）</summary>
    public List<Inspect_Plan> GetByMonth(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        return Repository.Find("[PlanDate]>=@s AND [PlanDate]<@e", new { s = start, e = end }, "[PlanDate] ASC").ToList();
    }
}

/// <summary>点检记录执行（含逐项结果与异常处置）</summary>
public class Inspect_RecordApp : BaseApp<Inspect_Record>
{
    private readonly IRepository<Inspect_RecordSub> _subRepo;
    private readonly IRepository<Inspect_Disposal> _dispRepo;

    public Inspect_RecordApp(IUnitWork unitWork, IRepository<Inspect_Record> repository,
        IRepository<Inspect_RecordSub> subRepo, IRepository<Inspect_Disposal> dispRepo)
        : base(unitWork, repository) { _subRepo = subRepo; _dispRepo = dispRepo; }

    public List<Inspect_RecordSub> GetSubs(long recordId)
        => _subRepo.Find("[RecordId]=@r", new { r = recordId }, "[Id] ASC").ToList();

    public List<Inspect_Disposal> GetDisposals(long recordId)
        => _dispRepo.Find("[RecordId]=@r", new { r = recordId }, "[Id] ASC").ToList();

    /// <summary>提交点检单：写记录 + 逐项结果；含异常自动置 Result=1</summary>
    public long Submit(Inspect_Record rec, IEnumerable<Inspect_RecordSub>? items)
    {
        var list = (items ?? Enumerable.Empty<Inspect_RecordSub>()).ToList();
        rec.Result = list.Any(x => !x.IsNormal) ? 1 : 0;
        if (rec.ExecTime == null) rec.ExecTime = DateTime.Now;
        if (rec.Id == 0)
        {
            rec.CreateDate = DateTime.Now;
            if (string.IsNullOrWhiteSpace(rec.RecordNo)) rec.RecordNo = "IR" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Repository.Insert(rec);
        }
        else Repository.Update(rec);

        var existed = _subRepo.Find("[RecordId]=@r", new { r = rec.Id }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) _subRepo.Delete(existed);
        foreach (var s in list)
        {
            s.Id = 0; s.RecordId = rec.Id;
            _subRepo.Insert(s);
        }
        return rec.Id;
    }

    /// <summary>异常处置分流（5 类）</summary>
    public long Dispatch(Inspect_Disposal d)
    {
        d.CreateDate = DateTime.Now;
        _dispRepo.Insert(d);
        return d.Id;
    }
}
