using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Wuziku")]
public class Wuziku : Entity
{
    public string Leibie { get; set; } = string.Empty;
    public string? Beizhu { get; set; }
    public string? Bianma { get; set; }
}
