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
        if (main.Id == 0)
        {
            main.CreateDate = DateTime.Now;
            if (main.Status == 0) main.Status = 1;
            if (string.IsNullOrWhiteSpace(main.StdNo)) main.StdNo = "IS" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Repository.Insert(main);
        }
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

/// <summary>点检计划：标准 × 多设备 × 周期，保存后逐台生成点检执行单（Inspect_Record）。</summary>
public class Inspect_PlanApp : BaseApp<Inspect_Plan>
{
    private readonly IRepository<Inspect_PlanDevice> _planDevRepo;
    private readonly IRepository<Inspect_Record> _recordRepo;
    private readonly IRepository<Inspect_Standard> _stdRepo;

    public Inspect_PlanApp(IUnitWork unitWork, IRepository<Inspect_Plan> repository,
        IRepository<Inspect_PlanDevice> planDevRepo, IRepository<Inspect_Record> recordRepo,
        IRepository<Inspect_Standard> stdRepo) : base(unitWork, repository)
    {
        _planDevRepo = planDevRepo;
        _recordRepo = recordRepo;
        _stdRepo = stdRepo;
    }

    public List<Inspect_PlanDevice> GetDevices(long planId)
        => _planDevRepo.Find("[PlanId]=@p", new { p = planId }, "[Id] ASC").ToList();

    /// <summary>保存计划并生成执行单。devices 为本计划覆盖的设备清单（Id + 名称）。</summary>
    public long SavePlan(Inspect_Plan m, IEnumerable<Inspect_PlanDevice>? devices)
    {
        var devList = (devices ?? Enumerable.Empty<Inspect_PlanDevice>())
            .Where(d => d.FacilityId > 0)
            .GroupBy(d => d.FacilityId).Select(g => g.First()).ToList();

        var isNew = m.Id == 0;
        if (isNew)
        {
            m.CreateDate = DateTime.Now;
            if (string.IsNullOrWhiteSpace(m.PlanNo)) m.PlanNo = "IP" + DateTime.Now.ToString("yyyyMMddHHmmss");
            if (m.Periods <= 0) m.Periods = 1;
            Repository.Insert(m);
        }
        else Repository.Update(m);

        // 重存设备关联（编辑时先清旧关联，不动已生成的历史执行单）
        var oldDev = _planDevRepo.Find("[PlanId]=@p", new { p = m.Id }).Select(x => x.Id).ToArray();
        if (oldDev.Length > 0) _planDevRepo.Delete(oldDev);
        foreach (var d in devList)
        {
            _planDevRepo.Insert(new Inspect_PlanDevice { PlanId = m.Id, FacilityId = d.FacilityId, FacilityName = d.FacilityName });
        }

        // 仅新建计划时生成执行单，避免编辑重复生成
        if (isNew) GenerateRecords(m, devList);
        return m.Id;
    }

    /// <summary>按「每台设备 × 每期」生成待执行点检单。</summary>
    private void GenerateRecords(Inspect_Plan plan, List<Inspect_PlanDevice> devices)
    {
        if (devices.Count == 0) return;
        var std = _stdRepo.FindSingle(plan.StandardId);
        var cycle = (std?.CycleType ?? plan.PlanNo) ?? "日";
        var count = plan.Periods <= 0 ? 1 : plan.Periods;
        var start = plan.PlanDate ?? DateTime.Now.Date;
        var seq = 0;

        foreach (var dev in devices)
        {
            for (var i = 0; i < count; i++)
            {
                var planDate = AddCycle(start, cycle, i);
                seq++;
                _recordRepo.Insert(new Inspect_Record
                {
                    RecordNo = $"IR{DateTime.Now:yyyyMMddHHmmss}{seq:D3}",
                    PlanId = plan.Id,
                    FacilityId = dev.FacilityId,
                    FacilityName = dev.FacilityName,
                    Executor = plan.Executor,
                    PlanDate = planDate,
                    ExecTime = null,
                    Result = 0,
                    CreateDate = DateTime.Now
                });
            }
        }
    }

    private static DateTime AddCycle(DateTime baseDate, string cycle, int i)
    {
        return (cycle ?? "").Trim() switch
        {
            "班" or "班次" => baseDate.AddDays(i),
            "日" => baseDate.AddDays(i),
            "周" => baseDate.AddDays(7 * i),
            "月" => baseDate.AddMonths(i),
            "季" or "季度" => baseDate.AddMonths(3 * i),
            "年" => baseDate.AddYears(i),
            _ => baseDate.AddDays(i)
        };
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

    /// <summary>提交点检单：写记录 + 逐项结果；含异常自动置 Result=1。更新已生成的待执行单时合并保留设备/计划等字段。</summary>
    public long Submit(Inspect_Record rec, IEnumerable<Inspect_RecordSub>? items)
    {
        var list = (items ?? Enumerable.Empty<Inspect_RecordSub>()).ToList();
        var result = list.Any(x => !x.IsNormal) ? 1 : 0;

        if (rec.Id == 0)
        {
            rec.Result = result;
            if (rec.ExecTime == null) rec.ExecTime = DateTime.Now;
            rec.CreateDate = DateTime.Now;
            if (string.IsNullOrWhiteSpace(rec.RecordNo)) rec.RecordNo = "IR" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Repository.Insert(rec);
        }
        else
        {
            // 加载已生成单，仅更新执行相关字段，保留设备/计划日期/记录号
            var exist = Repository.FindSingle(rec.Id) ?? rec;
            exist.Executor = string.IsNullOrWhiteSpace(rec.Executor) ? exist.Executor : rec.Executor;
            exist.Remark = rec.Remark;
            exist.Result = result;
            exist.ExecTime = DateTime.Now;
            Repository.Update(exist);
            rec = exist;
        }

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
