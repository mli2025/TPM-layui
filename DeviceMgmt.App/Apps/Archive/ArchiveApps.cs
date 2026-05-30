using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Archive;

/// <summary>FAT/SAT 验收（含问题跟踪）</summary>
public class Facility_AcceptanceApp : BaseApp<Facility_Acceptance>
{
    private readonly IRepository<Facility_AcceptanceIssue> _issueRepo;
    public Facility_AcceptanceApp(IUnitWork u, IRepository<Facility_Acceptance> r, IRepository<Facility_AcceptanceIssue> issueRepo) : base(u, r) { _issueRepo = issueRepo; }

    public List<Facility_AcceptanceIssue> GetIssues(long acceptId)
        => _issueRepo.Find("[AcceptId]=@a", new { a = acceptId }, "[Id] ASC").ToList();

    public long Save(Facility_Acceptance main, IEnumerable<Facility_AcceptanceIssue>? issues)
    {
        if (main.Id == 0) { main.CreateDate = DateTime.Now; if (string.IsNullOrWhiteSpace(main.BillNo)) main.BillNo = "AC" + DateTime.Now.ToString("yyyyMMddHHmmss"); Repository.Insert(main); }
        else Repository.Update(main);
        var existed = _issueRepo.Find("[AcceptId]=@a", new { a = main.Id }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) _issueRepo.Delete(existed);
        foreach (var s in issues ?? Enumerable.Empty<Facility_AcceptanceIssue>())
        {
            if (string.IsNullOrWhiteSpace(s.IssueDesc)) continue;
            s.Id = 0; s.AcceptId = main.Id; _issueRepo.Insert(s);
        }
        return main.Id;
    }

    public void DeleteCascade(long id)
    {
        var ids = _issueRepo.Find("[AcceptId]=@a", new { a = id }).Select(x => x.Id).ToArray();
        if (ids.Length > 0) _issueRepo.Delete(ids);
        Repository.Delete(id);
    }
}

/// <summary>设备盘点（主子）</summary>
public class Facility_StockCheckApp : BaseApp<Facility_StockCheck>
{
    private readonly IRepository<Facility_StockCheckSub> _subRepo;
    public Facility_StockCheckApp(IUnitWork u, IRepository<Facility_StockCheck> r, IRepository<Facility_StockCheckSub> subRepo) : base(u, r) { _subRepo = subRepo; }

    public List<Facility_StockCheckSub> GetSubs(long mainId)
        => _subRepo.Find("[MainId]=@m", new { m = mainId }, "[Id] ASC").ToList();

    public long Save(Facility_StockCheck main, IEnumerable<Facility_StockCheckSub>? subs)
    {
        if (main.Id == 0) { main.CreateDate = DateTime.Now; if (string.IsNullOrWhiteSpace(main.PlanNo)) main.PlanNo = "SC" + DateTime.Now.ToString("yyyyMMddHHmmss"); Repository.Insert(main); }
        else Repository.Update(main);
        var existed = _subRepo.Find("[MainId]=@m", new { m = main.Id }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) _subRepo.Delete(existed);
        foreach (var s in subs ?? Enumerable.Empty<Facility_StockCheckSub>())
        {
            if (s.FacilityId <= 0) continue;
            s.Id = 0; s.MainId = main.Id; _subRepo.Insert(s);
        }
        return main.Id;
    }

    public void DeleteCascade(long id)
    {
        var ids = _subRepo.Find("[MainId]=@m", new { m = id }).Select(x => x.Id).ToArray();
        if (ids.Length > 0) _subRepo.Delete(ids);
        Repository.Delete(id);
    }
}

public class Facility_AssetCardApp : BaseApp<Facility_AssetCard>
{
    public Facility_AssetCardApp(IUnitWork u, IRepository<Facility_AssetCard> r) : base(u, r) { }
    public long Save(Facility_AssetCard m)
    {
        if (m.Id == 0) { m.CreateDate = DateTime.Now; if (string.IsNullOrWhiteSpace(m.CardNo)) m.CardNo = "AS" + DateTime.Now.ToString("yyyyMMddHHmmss"); Repository.Insert(m); }
        else Repository.Update(m);
        return m.Id;
    }
}

public class Facility_CertApp : BaseApp<Facility_Cert>
{
    public Facility_CertApp(IUnitWork u, IRepository<Facility_Cert> r) : base(u, r) { }
    public long Save(Facility_Cert m)
    {
        if (m.WarnDays <= 0) m.WarnDays = 30;
        if (m.Id == 0) { if (m.Status == 0) m.Status = 1; Repository.Insert(m); } else Repository.Update(m);
        return m.Id;
    }
}

public class Facility_LabelApp : BaseApp<Facility_Label>
{
    public Facility_LabelApp(IUnitWork u, IRepository<Facility_Label> r) : base(u, r) { }
    public long Save(Facility_Label m)
    {
        if (string.IsNullOrWhiteSpace(m.LabelCode))
            m.LabelCode = "FAC-" + m.FacilityId + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        m.GenTime = DateTime.Now;
        if (m.Id == 0) Repository.Insert(m); else Repository.Update(m);
        return m.Id;
    }
}

public class Facility_LubeStandardApp : BaseApp<Facility_LubeStandard>
{
    public Facility_LubeStandardApp(IUnitWork u, IRepository<Facility_LubeStandard> r) : base(u, r) { }
    public long Save(Facility_LubeStandard m) { if (m.Id == 0) Repository.Insert(m); else Repository.Update(m); return m.Id; }
}

public class Facility_LubeRecordApp : BaseApp<Facility_LubeRecord>
{
    public Facility_LubeRecordApp(IUnitWork u, IRepository<Facility_LubeRecord> r) : base(u, r) { }
    public long Save(Facility_LubeRecord m)
    {
        if (m.Id == 0) { m.CreateDate = DateTime.Now; Repository.Insert(m); } else Repository.Update(m);
        return m.Id;
    }
}
