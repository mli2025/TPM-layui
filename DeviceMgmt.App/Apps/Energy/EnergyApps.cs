using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Energy;

public class Energy_PointApp : BaseApp<Energy_Point>
{
    public Energy_PointApp(IUnitWork unitWork, IRepository<Energy_Point> repository)
        : base(unitWork, repository) { }

    public long Save(Energy_Point e)
    {
        e.PointCode = (e.PointCode ?? string.Empty).Trim();
        var dup = Repository.Count("[PointCode]=@c AND [Id]<>@id", new { c = e.PointCode, id = e.Id });
        if (dup > 0) throw new InvalidOperationException("计量点编号已存在");
        if (e.Id == 0) { if (e.Status == 0) e.Status = 1; Repository.Insert(e); }
        else Repository.Update(e);
        return e.Id;
    }

    /// <summary>实时监控仪表板：每个计量点取最新一条采集值</summary>
    public IEnumerable<dynamic> Dashboard()
    {
        const string sql = @"
SELECT p.[Id], p.[PointCode], p.[MediaType], p.[MeterModel], p.[Status],
       d.[InstValue], d.[AccuValue], d.[Ts]
FROM [Energy_Point] p
OUTER APPLY (
    SELECT TOP 1 [InstValue],[AccuValue],[Ts]
    FROM [Energy_RealtimeData] r
    WHERE r.[PointId]=p.[Id]
    ORDER BY r.[Ts] DESC
) d
WHERE p.[Status]=1
ORDER BY p.[PointCode]";
        return Repository.Query<dynamic>(sql);
    }
}

public class Energy_SummaryApp : BaseApp<Energy_Summary>
{
    public Energy_SummaryApp(IUnitWork unitWork, IRepository<Energy_Summary> repository)
        : base(unitWork, repository) { }
}

public class Energy_AlarmRuleApp : BaseApp<Energy_AlarmRule>
{
    public Energy_AlarmRuleApp(IUnitWork unitWork, IRepository<Energy_AlarmRule> repository)
        : base(unitWork, repository) { }
}

public class Energy_AlarmRecordApp : BaseApp<Energy_AlarmRecord>
{
    public Energy_AlarmRecordApp(IUnitWork unitWork, IRepository<Energy_AlarmRecord> repository)
        : base(unitWork, repository) { }

    /// <summary>处置报警记录（标记已处理）</summary>
    public void Handle(long id)
        => Repository.ExecuteSql("UPDATE [Energy_AlarmRecord] SET [HandleStatus]=1 WHERE [Id]=@id", new { id });
}
