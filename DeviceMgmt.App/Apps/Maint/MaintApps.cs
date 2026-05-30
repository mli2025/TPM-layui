using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.Maint;

/// <summary>维保标准库（主子）</summary>
public class Maint_StandardApp : BaseApp<Maint_Standard>
{
    private readonly IRepository<Maint_StandardSub> _subRepo;

    public Maint_StandardApp(IUnitWork unitWork, IRepository<Maint_Standard> repository, IRepository<Maint_StandardSub> subRepo)
        : base(unitWork, repository)
    {
        _subRepo = subRepo;
    }

    public List<Maint_StandardSub> GetSubs(long mainId)
        => _subRepo.Find("[MainId]=@m", new { m = mainId }, "[Sort] ASC,[Id] ASC").ToList();

    public long Save(Maint_Standard main, IEnumerable<Maint_StandardSub>? subs)
    {
        if (main.Id == 0) { main.CreateDate = DateTime.Now; if (main.Status == 0) main.Status = 1; Repository.Insert(main); }
        else Repository.Update(main);

        var existed = _subRepo.Find("[MainId]=@m", new { m = main.Id }).Select(x => x.Id).ToArray();
        if (existed.Length > 0) _subRepo.Delete(existed);
        var sort = 1;
        foreach (var s in subs ?? Enumerable.Empty<Maint_StandardSub>())
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

/// <summary>维保延期申请</summary>
public class Maint_DelayApplyApp : BaseApp<Maint_DelayApply>
{
    public Maint_DelayApplyApp(IUnitWork unitWork, IRepository<Maint_DelayApply> repository) : base(unitWork, repository) { }

    public long SaveApply(Maint_DelayApply m)
    {
        if (m.Id == 0) { m.CreateDate = DateTime.Now; m.ApproveStatus = 0; Repository.Insert(m); }
        else Repository.Update(m);
        return m.Id;
    }

    public void Approve(long id, bool agree)
        => Repository.ExecuteSql("UPDATE [Maint_DelayApply] SET [ApproveStatus]=@s WHERE [Id]=@id",
            new { s = agree ? 1 : 2, id });
}

/// <summary>维保资质有效期监控</summary>
public class Maint_QualificationApp : BaseApp<Maint_Qualification>
{
    public Maint_QualificationApp(IUnitWork unitWork, IRepository<Maint_Qualification> repository) : base(unitWork, repository) { }

    public long SaveQual(Maint_Qualification m)
    {
        if (m.WarnDays <= 0) m.WarnDays = 30;
        if (m.Id == 0) { if (m.Status == 0) m.Status = 1; Repository.Insert(m); }
        else Repository.Update(m);
        return m.Id;
    }
}
