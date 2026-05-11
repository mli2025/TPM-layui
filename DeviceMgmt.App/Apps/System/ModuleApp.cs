using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;

namespace DeviceMgmt.App.Apps.System;

public class ModuleApp : BaseApp<Sys_Module>
{
    private readonly IRepository<Sys_ModuleButtons> _buttonRepo;
    private readonly IRepository<Sys_UserRole> _userRoleRepo;
    private readonly IRepository<Sys_RoleModule> _roleModuleRepo;

    public ModuleApp(IUnitWork unitWork,
        IRepository<Sys_Module> repository,
        IRepository<Sys_ModuleButtons> buttonRepo,
        IRepository<Sys_UserRole> userRoleRepo,
        IRepository<Sys_RoleModule> roleModuleRepo) : base(unitWork, repository)
    {
        _buttonRepo = buttonRepo;
        _userRoleRepo = userRoleRepo;
        _roleModuleRepo = roleModuleRepo;
    }

    public List<Sys_Module> GetModulesByUser(long userId)
    {
        var roleIds = _userRoleRepo.Find("[UserId]=@uid", new { uid = userId }).Select(x => x.RoleId).ToArray();
        if (roleIds.Length == 0) return Repository.Find().ToList();
        var moduleIds = _roleModuleRepo.Find("[RoleId] IN @rids", new { rids = roleIds }).Select(x => x.ModuleId).Distinct().ToArray();
        if (moduleIds.Length == 0) return new List<Sys_Module>();
        return Repository.Find("[Id] IN @mids", new { mids = moduleIds }, "[Sort] ASC").ToList();
    }

    public List<Sys_ModuleButtons> GetButtonsByUser(long userId)
    {
        var modules = GetModulesByUser(userId);
        if (modules.Count == 0) return new List<Sys_ModuleButtons>();
        var ids = modules.Select(m => m.Id).ToArray();
        return _buttonRepo.Find("[ModuleId] IN @mids", new { mids = ids }).ToList();
    }
}
