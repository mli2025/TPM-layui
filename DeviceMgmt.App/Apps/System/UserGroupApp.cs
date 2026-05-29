using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

/// <summary>
/// 用户组（URS 408）：组 CRUD + 组成员绑定 + 组-菜单权限绑定。
/// 权限叠加：用户最终权限 = 直属角色权限 ∪ 所属各用户组权限。
/// </summary>
public class UserGroupApp : BaseApp<Sys_UserGroup>
{
    private readonly IRepository<Sys_UserGroupUser> _groupUserRepo;
    private readonly IRepository<Sys_UserGroupModule> _groupModuleRepo;

    public UserGroupApp(
        IUnitWork unitWork,
        IRepository<Sys_UserGroup> repository,
        IRepository<Sys_UserGroupUser> groupUserRepo,
        IRepository<Sys_UserGroupModule> groupModuleRepo) : base(unitWork, repository)
    {
        _groupUserRepo = groupUserRepo;
        _groupModuleRepo = groupModuleRepo;
    }

    public long[] GetGroupModuleIds(long groupId)
        => _groupModuleRepo.Find("[GroupId]=@g", new { g = groupId }).Select(x => x.ModuleId).ToArray();

    public long[] GetGroupUserIds(long groupId)
        => _groupUserRepo.Find("[GroupId]=@g", new { g = groupId }).Select(x => x.UserId).ToArray();

    public void SetGroupModules(long groupId, long[] moduleIds)
    {
        var existed = _groupModuleRepo.Find("[GroupId]=@g", new { g = groupId }).ToList();
        var existedIds = existed.Select(x => x.ModuleId).ToHashSet();
        var newIds = new HashSet<long>(moduleIds ?? Array.Empty<long>());

        var toRemove = existed.Where(x => !newIds.Contains(x.ModuleId)).Select(x => x.Id).ToArray();
        if (toRemove.Length > 0) _groupModuleRepo.Delete(toRemove);

        foreach (var mid in newIds.Where(m => !existedIds.Contains(m)))
            _groupModuleRepo.Insert(new Sys_UserGroupModule { GroupId = groupId, ModuleId = mid });
    }

    public void SetGroupUsers(long groupId, long[] userIds)
    {
        var existed = _groupUserRepo.Find("[GroupId]=@g", new { g = groupId }).ToList();
        var existedIds = existed.Select(x => x.UserId).ToHashSet();
        var newIds = new HashSet<long>(userIds ?? Array.Empty<long>());

        var toRemove = existed.Where(x => !newIds.Contains(x.UserId)).Select(x => x.Id).ToArray();
        if (toRemove.Length > 0) _groupUserRepo.Delete(toRemove);

        foreach (var uid in newIds.Where(u => !existedIds.Contains(u)))
            _groupUserRepo.Insert(new Sys_UserGroupUser { GroupId = groupId, UserId = uid });
    }

    public long SaveGroup(Sys_UserGroup group)
    {
        group.Name = (group.Name ?? string.Empty).Trim();
        if (group.Id == 0)
        {
            if (group.Status == 0) group.Status = 1;
            group.CreateDate = DateTime.Now;
            Repository.Insert(group);
        }
        else
        {
            Repository.Update(group);
        }
        return group.Id;
    }

    public void DeleteGroupCascade(long groupId)
    {
        SetGroupModules(groupId, Array.Empty<long>());
        SetGroupUsers(groupId, Array.Empty<long>());
        Repository.Delete(groupId);
    }
}
