using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

/// <summary>FAT/SAT 验收（URS 501-532）</summary>
[Table("Facility_Acceptance")]
public class Facility_Acceptance : Entity
{
    public string? BillNo { get; set; }
    public long FacilityId { get; set; }
    public string? AcceptType { get; set; }      // FAT/SAT
    public bool? AppearanceOK { get; set; }
    public bool? QtyOK { get; set; }
    public bool? DocOK { get; set; }
    public bool? FunctionOK { get; set; }
    public int Result { get; set; }               // 0待验/1通过/2不通过
    public DateTime? AcceptDate { get; set; }
    public string? Acceptor { get; set; }
    public string? Remark { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>验收问题跟踪</summary>
[Table("Facility_AcceptanceIssue")]
public class Facility_AcceptanceIssue : Entity
{
    public long AcceptId { get; set; }
    public string? IssueDesc { get; set; }
    public string? Solution { get; set; }
    public string? Owner { get; set; }
    public int Status { get; set; }
    public DateTime? CloseDate { get; set; }
}

/// <summary>设备盘点（计划主）</summary>
[Table("Facility_StockCheck")]
public class Facility_StockCheck : Entity
{
    public string? PlanNo { get; set; }
    public string? PlanName { get; set; }
    public DateTime? PlanDate { get; set; }
    public string? Owner { get; set; }
    public int Status { get; set; }               // 0计划/1执行中/2完成
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>设备盘点明细</summary>
[Table("Facility_StockCheckSub")]
public class Facility_StockCheckSub : Entity
{
    public long MainId { get; set; }
    public long FacilityId { get; set; }
    public string? RealStatus { get; set; }
    public string? DiffDesc { get; set; }
    public DateTime? CheckTime { get; set; }
    public string? Checker { get; set; }
}

/// <summary>资产卡片</summary>
[Table("Facility_AssetCard")]
public class Facility_AssetCard : Entity
{
    public long? FacilityId { get; set; }
    public string? CardNo { get; set; }
    public string? AssetName { get; set; }
    public string? Specs { get; set; }
    public long? DeptId { get; set; }
    public string? Location { get; set; }
    public decimal? OriginalValue { get; set; }
    public string? DepreMethod { get; set; }
    public decimal? DepreYears { get; set; }
    public decimal? NetValue { get; set; }
    public DateTime? SyncDate { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}

/// <summary>证书/许可时效</summary>
[Table("Facility_Cert")]
public class Facility_Cert : Entity
{
    public long FacilityId { get; set; }
    public string? CertName { get; set; }
    public string? Issuer { get; set; }
    public DateTime? EffectDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public int WarnDays { get; set; } = 30;
    public int Status { get; set; } = 1;
}

/// <summary>标签（二维码/条码）</summary>
[Table("Facility_Label")]
public class Facility_Label : Entity
{
    public long FacilityId { get; set; }
    public string? LabelType { get; set; }        // qrcode/barcode/nfc/rfid
    public string? LabelCode { get; set; }
    public DateTime GenTime { get; set; } = DateTime.Now;
}

/// <summary>润滑标准</summary>
[Table("Facility_LubeStandard")]
public class Facility_LubeStandard : Entity
{
    public string? FacilityType { get; set; }
    public string? LubePart { get; set; }
    public string? RecommendOil { get; set; }
    public int? CycleDays { get; set; }
    public string? Remark { get; set; }
}

/// <summary>润滑记录</summary>
[Table("Facility_LubeRecord")]
public class Facility_LubeRecord : Entity
{
    public long FacilityId { get; set; }
    public string? LubePart { get; set; }
    public string? OilModel { get; set; }
    public int? CycleDays { get; set; }
    public DateTime? LastDate { get; set; }
    public string? Executor { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
