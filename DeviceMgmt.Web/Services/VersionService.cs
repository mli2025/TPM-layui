using DeviceMgmt.Repository.Core;
using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.Web.Services;

public sealed class VersionService
{
    private readonly IRepository<Sys_Version> _repo;

    public VersionService(IRepository<Sys_Version> repo)
    {
        _repo = repo;
    }

    public Sys_Version? Current()
        => _repo.FindSingle("[IsCurrent]=1");

    public IEnumerable<Sys_Version> Timeline()
        => _repo.Find(null, null, "[ReleaseDate] DESC, [Id] DESC");

    public long Publish(Sys_Version v)
    {
        _repo.ExecuteSql("UPDATE [Sys_Version] SET [IsCurrent]=0");
        v.IsCurrent = true;
        if (v.CreateDate == default) v.CreateDate = DateTime.Now;
        if (v.ReleaseDate == default) v.ReleaseDate = DateTime.Now;
        return _repo.Insert(v);
    }

    public int Update(Sys_Version v)
    {
        if (v.IsCurrent)
        {
            _repo.ExecuteSql("UPDATE [Sys_Version] SET [IsCurrent]=0 WHERE [Id]<>@id", new { id = v.Id });
        }
        return _repo.Update(v);
    }

    public int Delete(long id) => _repo.Delete(id);
}
