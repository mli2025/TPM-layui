using DeviceMgmt.Repository.Domain;
using DeviceMgmt.Repository.Interface;
using Microsoft.Data.SqlClient;

namespace DeviceMgmt.App.Apps.System;

public class ModuleApp : BaseApp<Sys_Module>
{
    private readonly IRepository<Sys_ModuleButtons> _buttonRepo;
    private readonly IRepository<Sys_UserRole> _userRoleRepo;
    private readonly IRepository<Sys_RoleModule> _roleModuleRepo;
    private readonly IRepository<Sys_UserGroupUser> _groupUserRepo;
    private readonly IRepository<Sys_UserGroupModule> _groupModuleRepo;

    public ModuleApp(IUnitWork unitWork,
        IRepository<Sys_Module> repository,
        IRepository<Sys_ModuleButtons> buttonRepo,
        IRepository<Sys_UserRole> userRoleRepo,
        IRepository<Sys_RoleModule> roleModuleRepo,
        IRepository<Sys_UserGroupUser> groupUserRepo,
        IRepository<Sys_UserGroupModule> groupModuleRepo) : base(unitWork, repository)
    {
        _buttonRepo = buttonRepo;
        _userRoleRepo = userRoleRepo;
        _roleModuleRepo = roleModuleRepo;
        _groupUserRepo = groupUserRepo;
        _groupModuleRepo = groupModuleRepo;
    }

    /// <summary>仅返回启用菜单（Status=1），停用项不进入侧边栏。</summary>
    private static List<Sys_Module> OnlyEnabled(IEnumerable<Sys_Module> modules)
        => modules.Where(m => m.Status == 1).ToList();

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
            return OnlyEnabled(Repository.Find());
        }

        // URS 408：用户最终权限 = 直属角色权限 ∪ 所属各用户组权限（并集）
        var moduleIdSet = new HashSet<long>();

        if (roleIds.Length > 0)
        {
            try
            {
                foreach (var mid in _roleModuleRepo.Find("[RoleId] IN @rids", new { rids = roleIds }).Select(x => x.ModuleId))
                    moduleIdSet.Add(mid);
            }
            catch (SqlException ex) when (ex.Number == 208) { return OnlyEnabled(Repository.Find()); }
        }

        try
        {
            var groupIds = _groupUserRepo.Find("[UserId]=@uid", new { uid = userId }).Select(x => x.GroupId).ToArray();
            if (groupIds.Length > 0)
            {
                foreach (var mid in _groupModuleRepo.Find("[GroupId] IN @gids", new { gids = groupIds }).Select(x => x.ModuleId))
                    moduleIdSet.Add(mid);
            }
        }
        catch (SqlException ex) when (ex.Number == 208) { /* 用户组表缺失则忽略组权限 */ }

        // 既无角色也无用户组绑定：默认放开全部启用菜单（兼容旧库/admin 初始）
        if (roleIds.Length == 0 && moduleIdSet.Count == 0) return OnlyEnabled(Repository.Find());
        if (moduleIdSet.Count == 0) return new List<Sys_Module>();

        try
        {
            return OnlyEnabled(Repository.Find("[Id] IN @mids", new { mids = moduleIdSet.ToArray() }, "[Sort] ASC"));
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            return OnlyEnabled(Repository.Find());
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

    public long SaveModule(Sys_Module m)
    {
        if (m.Id == 0)
        {
            if (m.Status == 0) m.Status = 1;
            Repository.Insert(m);
        }
        else
        {
            Repository.Update(m);
        }
        return m.Id;
    }

    public List<Sys_ModuleButtons> GetButtons(long moduleId)
    {
        return _buttonRepo.Find("[ModuleId]=@m", new { m = moduleId }, "[Id] ASC").ToList();
    }

    public long SaveButton(Sys_ModuleButtons b)
    {
        if (b.Id == 0) _buttonRepo.Insert(b);
        else _buttonRepo.Update(b);
        return b.Id;
    }

    public void DeleteButton(long id) => _buttonRepo.Delete(id);
}
