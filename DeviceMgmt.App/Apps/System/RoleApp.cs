using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Infrastructure;

namespace DeviceMgmt.App.Apps.System;

public class RoleApp : BaseApp<Sys_Role>
{
    private readonly IRepository<Sys_RoleModule> _roleModuleRepo;
    private readonly IRepository<Sys_UserRole> _userRoleRepo;

    public RoleApp(
        IUnitWork unitWork,
        IRepository<Sys_Role> repository,
        IRepository<Sys_RoleModule> roleModuleRepo,
        IRepository<Sys_UserRole> userRoleRepo) : base(unitWork, repository)
    {
        _roleModuleRepo = roleModuleRepo;
        _userRoleRepo = userRoleRepo;
    }

    public long[] GetRoleModuleIds(long roleId)
    {
        return _roleModuleRepo.Find("[RoleId]=@r", new { r = roleId })
            .Select(x => x.ModuleId).ToArray();
    }

    public long[] GetUserRoleIds(long userId)
    {
        return _userRoleRepo.Find("[UserId]=@u", new { u = userId })
            .Select(x => x.RoleId).ToArray();
    }

    public void SetRoleModules(long roleId, long[] moduleIds)
    {
        var existed = _roleModuleRepo.Find("[RoleId]=@r", new { r = roleId }).ToList();
        var existedIds = existed.Select(x => x.ModuleId).ToHashSet();
        var newIds = new HashSet<long>(moduleIds ?? Array.Empty<long>());

        var toRemove = existed.Where(x => !newIds.Contains(x.ModuleId)).Select(x => x.Id).ToArray();
        if (toRemove.Length > 0) _roleModuleRepo.Delete(toRemove);

        foreach (var mid in newIds.Where(m => !existedIds.Contains(m)))
        {
            _roleModuleRepo.Insert(new Sys_RoleModule { RoleId = roleId, ModuleId = mid });
        }
    }

    public void SetUserRoles(long userId, long[] roleIds)
    {
        var existed = _userRoleRepo.Find("[UserId]=@u", new { u = userId }).ToList();
        var existedIds = existed.Select(x => x.RoleId).ToHashSet();
        var newIds = new HashSet<long>(roleIds ?? Array.Empty<long>());

        var toRemove = existed.Where(x => !newIds.Contains(x.RoleId)).Select(x => x.Id).ToArray();
        if (toRemove.Length > 0) _userRoleRepo.Delete(toRemove);

        foreach (var rid in newIds.Where(r => !existedIds.Contains(r)))
        {
            _userRoleRepo.Insert(new Sys_UserRole { UserId = userId, RoleId = rid });
        }
    }

    public long SaveRole(Sys_Role role)
    {
        if (role.Id == 0)
        {
            if (role.Status == 0) role.Status = 1;
            Repository.Insert(role);
        }
        else
        {
            Repository.Update(role);
        }
        return role.Id;
    }
}
