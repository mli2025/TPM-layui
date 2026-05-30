using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>备件多级阈值预警配置（最低/再订货点/安全/最高）</summary>
[Table("Spare_AlarmConfig")]
public class Spare_AlarmConfig : Entity
{
    public long SpareId { get; set; }
    public decimal? MinStock { get; set; }       // 最低库存（严重）
    public decimal? ReorderPoint { get; set; }   // 再订货点（警告）
    public decimal? SafeStock { get; set; }      // 安全库存（提示）
    public decimal? MaxStock { get; set; }       // 最高库存（超储）
    public int Enabled { get; set; } = 1;
}

/// <summary>备件生命周期事件跟踪</summary>
[Table("Spare_LifeCycle")]
public class Spare_LifeCycle : Entity
{
    public long SpareId { get; set; }
    public string? EventType { get; set; }       // 采购入库/领用出库/报废/退库/盘盈/盘亏
    public DateTime EventDate { get; set; } = DateTime.Now;
    public decimal? Qty { get; set; }
    public string? RefBillNo { get; set; }
    public string? Operator { get; set; }
    public string? Remark { get; set; }
}

/// <summary>备件盘点（主）</summary>
[Table("Spare_StockCheck")]
public class Spare_StockCheck : Entity
{
    public string? PlanNo { get; set; }
    public string? PlanName { get; set; }
    public DateTime? PlanDate { get; set; }
    public string? Owner { get; set; }
    public int Status { get; set; }              // 0计划/1执行中/2完成
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>备件盘点明细</summary>
[Table("Spare_StockCheckSub")]
public class Spare_StockCheckSub : Entity
{
    public long MainId { get; set; }
    public long SpareId { get; set; }
    public decimal? BookQty { get; set; }        // 账面数
    public decimal? RealQty { get; set; }        // 实盘数
    public decimal? DiffQty { get; set; }        // 差异
    public string? Remark { get; set; }
}
