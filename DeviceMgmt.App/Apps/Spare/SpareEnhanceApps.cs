using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Spare;

/// <summary>备件多级预警配置 + 预警计算</summary>
public class Spare_AlarmConfigApp : BaseApp<Spare_AlarmConfig>
{
    public Spare_AlarmConfigApp(IUnitWork u, IRepository<Spare_AlarmConfig> r) : base(u, r) { }

    public long Save(Spare_AlarmConfig m)
    {
        if (m.Id == 0) Repository.Insert(m); else Repository.Update(m);
        return m.Id;
    }

    /// <summary>预警清单：现存量按备件汇总，对照配置判级</summary>
    public List<dynamic> AlarmList()
        => Repository.Query<dynamic>(@"
            SELECT s.[Id] AS SpareId, s.[Code], s.[Name], s.[Specs],
                   ISNULL(q.Qty,0) AS Qty,
                   c.[MinStock], c.[ReorderPoint], c.[SafeStock], c.[MaxStock],
                   CASE
                     WHEN c.[MinStock] IS NOT NULL AND ISNULL(q.Qty,0) <= c.[MinStock] THEN N'严重'
                     WHEN c.[ReorderPoint] IS NOT NULL AND ISNULL(q.Qty,0) <= c.[ReorderPoint] THEN N'警告'
                     WHEN c.[SafeStock] IS NOT NULL AND ISNULL(q.Qty,0) <= c.[SafeStock] THEN N'提示'
                     WHEN c.[MaxStock] IS NOT NULL AND ISNULL(q.Qty,0) >= c.[MaxStock] THEN N'超储'
                     ELSE N'正常'
                   END AS AlarmLevel
            FROM [Basic_Spare] s
            INNER JOIN [Spare_AlarmConfig] c ON c.[SpareId]=s.[Id] AND c.[Enabled]=1
            OUTER APPLY (SELECT SUM([Qty]) AS Qty FROM [Spare_NowQuan] WHERE [SpareId]=s.[Id]) q
            ORDER BY CASE
                     WHEN c.[MinStock] IS NOT NULL AND ISNULL(q.Qty,0) <= c.[MinStock] THEN 1
                     WHEN c.[ReorderPoint] IS NOT NULL AND ISNULL(q.Qty,0) <= c.[ReorderPoint] THEN 2
                     WHEN c.[SafeStock] IS NOT NULL AND ISNULL(q.Qty,0) <= c.[SafeStock] THEN 3
                     ELSE 9 END").ToList();

    /// <summary>可出库量：现存量 - 预留（预留预测留接口，当前预留=0）</summary>
    public List<dynamic> AvailableList()
        => Repository.Query<dynamic>(@"
            SELECT s.[Id] AS SpareId, s.[Code], s.[Name], s.[Specs],
                   ISNULL(q.Qty,0) AS OnHandQty,
                   0 AS ReservedQty,
                   ISNULL(q.Qty,0) - 0 AS AvailableQty
            FROM [Basic_Spare] s
            OUTER APPLY (SELECT SUM([Qty]) AS Qty FROM [Spare_NowQuan] WHERE [SpareId]=s.[Id]) q
            WHERE ISNULL(s.[Status],1)=1
            ORDER BY s.[Code]").ToList();
}

/// <summary>备件生命周期</summary>
public class Spare_LifeCycleApp : BaseApp<Spare_LifeCycle>
{
    public Spare_LifeCycleApp(IUnitWork u, IRepository<Spare_LifeCycle> r) : base(u, r) { }
    public long Save(Spare_LifeCycle m)
    {
        if (m.EventDate == default) m.EventDate = DateTime.Now;
        if (m.Id == 0) Repository.Insert(m); else Repository.Update(m);
        return m.Id;
    }
    public List<Spare_LifeCycle> GetBySpare(long spareId)
        => Repository.Find("[SpareId]=@s", new { s = spareId }, "[EventDate] DESC,[Id] DESC").ToList();
}

/// <summary>备件盘点（主子）</summary>
public class Spare_StockCheckApp : BaseApp<Spare_StockCheck>
{
    private readonly IRepository<Spare_StockCheckSub> _subRepo;
    public Spare_StockCheckApp(IUnitWork u, IRepository<Spare_StockCheck> r, IRepository<Spare_StockCheckSub> subRepo) : base(u, r) { _subRepo = subRepo; }

    public List<Spare_StockCheckSub> GetSubs(long mainId)
        => _subRepo.Find("[MainId]=@m", new { m = mainId }, "[Id] ASC").ToList();

    public long Save(Spare_StockCheck main, IEnumerable<Spare_StockCheckSub>? subs)
    {
        if (main.Id == 0) { main.CreateDate = DateTime.Now; if (string.IsNullOrWhiteSpace(main.PlanNo)) main.PlanNo = "SPC" + DateTime.Now.ToString("yyyyMMddHHmmss"); Repository.Insert(main); }
        else Repository.Update(main);
        var existed = _subRepo.Find("[MainId]=@m", new { m = main.Id }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) _subRepo.Delete(existed);
        foreach (var s in subs ?? Enumerable.Empty<Spare_StockCheckSub>())
        {
            if (s.SpareId <= 0) continue;
            s.Id = 0; s.MainId = main.Id;
            s.DiffQty = (s.RealQty ?? 0) - (s.BookQty ?? 0);
            _subRepo.Insert(s);
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
