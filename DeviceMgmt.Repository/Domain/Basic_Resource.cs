using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>生产资源/工作中心资源（MES 基础资料，设备台账 ResourceId 外键）</summary>
[Table("Basic_Resource")]
public class Basic_Resource : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
}
