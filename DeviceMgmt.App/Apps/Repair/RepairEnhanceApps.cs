using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Repair;

public class Facility_RepairTemplateApp : BaseApp<Facility_RepairTemplate>
{
    public Facility_RepairTemplateApp(IUnitWork u, IRepository<Facility_RepairTemplate> r) : base(u, r) { }
    public long Save(Facility_RepairTemplate m)
    {
        if (m.Id == 0) { m.CreateDate = DateTime.Now; if (m.Status == 0) m.Status = 1; Repository.Insert(m); }
        else Repository.Update(m);
        return m.Id;
    }
}

/// <summary>维修费用分摊（按工单批量维护）</summary>
public class Facility_RepairCostApp : BaseApp<Facility_RepairCost>
{
    public Facility_RepairCostApp(IUnitWork u, IRepository<Facility_RepairCost> r) : base(u, r) { }

    public List<Facility_RepairCost> GetByBill(long billId)
        => Repository.Find("[RepairBillId]=@b", new { b = billId }, "[Id] ASC").ToList();

    public void SaveBatch(long billId, IEnumerable<Facility_RepairCost>? rows)
    {
        var existed = Repository.Find("[RepairBillId]=@b", new { b = billId }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) Repository.Delete(existed);
        foreach (var r in rows ?? Enumerable.Empty<Facility_RepairCost>())
        {
            if (r.FacilityId <= 0) continue;
            r.Id = 0; r.RepairBillId = billId; Repository.Insert(r);
        }
    }
}

public class Facility_AlarmRuleApp : BaseApp<Facility_AlarmRule>
{
    public Facility_AlarmRuleApp(IUnitWork u, IRepository<Facility_AlarmRule> r) : base(u, r) { }
    public long Save(Facility_AlarmRule m)
    {
        if (m.Id == 0) { m.CreateDate = DateTime.Now; Repository.Insert(m); } else Repository.Update(m);
        return m.Id;
    }
}

public class Facility_AlarmRecordApp : BaseApp<Facility_AlarmRecord>
{
    public Facility_AlarmRecordApp(IUnitWork u, IRepository<Facility_AlarmRecord> r) : base(u, r) { }
    public long Save(Facility_AlarmRecord m)
    {
        if (m.Id == 0) { if (m.AlarmTime == default) m.AlarmTime = DateTime.Now; Repository.Insert(m); } else Repository.Update(m);
        return m.Id;
    }
    public void Handle(long id, string? handler, string? remark)
        => Repository.ExecuteSql("UPDATE [Facility_AlarmRecord] SET [Handled]=1,[Handler]=@h,[HandleRemark]=@r WHERE [Id]=@id",
            new { h = handler, r = remark, id });
}

/// <summary>维修故障看板 + 统计分析（基于 Facility_RepairBillMain 聚合）</summary>
public class RepairStatApp
{
    private readonly IRepository<Facility_RepairBillMain> _repo;
    public RepairStatApp(IRepository<Facility_RepairBillMain> repo) { _repo = repo; }

    /// <summary>看板卡片：总数 / 未关闭 / 本月新增 / 外委</summary>
    public object Board()
    {
        var statusRows = _repo.Query<dynamic>("SELECT ISNULL([Status],0) AS k, COUNT(*) AS c FROM [Facility_RepairBillMain] GROUP BY [Status]").ToList();
        var catRows = _repo.Query<dynamic>("SELECT ISNULL([FaultCategory],N'未分类') AS k, COUNT(*) AS c FROM [Facility_RepairBillMain] GROUP BY [FaultCategory]").ToList();
        var total = _repo.Query<dynamic>("SELECT COUNT(*) AS c FROM [Facility_RepairBillMain]").FirstOrDefault();
        var month = _repo.Query<dynamic>("SELECT COUNT(*) AS c FROM [Facility_RepairBillMain] WHERE [BillDate]>=@s", new { s = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) }).FirstOrDefault();
        var outsrc = _repo.Query<dynamic>("SELECT COUNT(*) AS c FROM [Facility_RepairBillMain] WHERE [OutsourcingFlag]=1").FirstOrDefault();
        return new { byStatus = statusRows, byCategory = catRows, total, month, outsrc };
    }

    /// <summary>月度趋势（近 12 月新增工单数）</summary>
    public object MonthlyTrend()
        => _repo.Query<dynamic>(@"SELECT FORMAT([BillDate],'yyyy-MM') AS ym, COUNT(*) AS c
            FROM [Facility_RepairBillMain]
            WHERE [BillDate] >= DATEADD(MONTH,-11,DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1))
            GROUP BY FORMAT([BillDate],'yyyy-MM') ORDER BY ym").ToList();

    /// <summary>MTTR：平均修复时长（小时），按故障分类</summary>
    public object MTTRByCategory()
        => _repo.Query<dynamic>(@"SELECT ISNULL([FaultCategory],N'未分类') AS k,
                AVG(CAST(DATEDIFF(MINUTE,[RepairBeginDate],[RepairEndDate]) AS float))/60.0 AS mttrHours,
                COUNT(*) AS c
            FROM [Facility_RepairBillMain]
            WHERE [RepairBeginDate] IS NOT NULL AND [RepairEndDate] IS NOT NULL
            GROUP BY [FaultCategory]").ToList();
}
