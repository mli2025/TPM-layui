using DeviceMgmt.Repository.Domain;

namespace DeviceMgmt.App.AuthStrategies;

public class AuthStrategyContext
{
    public Sys_User User { get; set; } = new();
    public List<Sys_Module> Modules { get; set; } = new();
    public List<Sys_ModuleButtons> ModuleElements { get; set; } = new();
}
