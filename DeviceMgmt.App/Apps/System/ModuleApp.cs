using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Microsoft.Data.SqlClient;

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
        long[] roleIds;
        try
        {
            roleIds = _userRoleRepo.Find("[UserId]=@uid", new { uid = userId }).Select(x => x.RoleId).ToArray();
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            // Table missing (e.g. hamaton DB has Sys_User but not Sys_UserRole) — show all modules
            return Repository.Find().ToList();
        }

        if (roleIds.Length == 0) return Repository.Find().ToList();

        long[] moduleIds;
        try
        {
            moduleIds = _roleModuleRepo.Find("[RoleId] IN @rids", new { rids = roleIds }).Select(x => x.ModuleId).Distinct().ToArray();
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Repository.Find().ToList();
        }

        if (moduleIds.Length == 0) return new List<Sys_Module>();

        try
        {
            return Repository.Find("[Id] IN @mids", new { mids = moduleIds }, "[Sort] ASC").ToList();
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return Repository.Find().ToList();
        }
    }

    public List<Sys_ModuleButtons> GetButtonsByUser(long userId)
    {
        var modules = GetModulesByUser(userId);
        if (modules.Count == 0) return new List<Sys_ModuleButtons>();
        var ids = modules.Select(m => m.Id).ToArray();
        try
        {
            return _buttonRepo.Find("[ModuleId] IN @mids", new { mids = ids }).ToList();
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return new List<Sys_ModuleButtons>();
        }
    }
}
