using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_ResourceDetail")]
public class Facility_ResourceDetail : Entity
{
    public string FacilityCode { get; set; } = string.Empty;
    public string FacilityName { get; set; } = string.Empty;
    public string FacilityType { get; set; } = string.Empty;
    public long ResourceId { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public DateTime ManufacturerDate { get; set; } = DateTime.Now;
    public string ManufactureCountry { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime? ExpireDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string? EquipmentManual { get; set; }
    public string? EquipmentDrawing { get; set; }
    public string Location { get; set; } = string.Empty;
    public long DeptId { get; set; }
    public string? AssetNumber { get; set; }
    public int? Voltage { get; set; }
    public string? Size { get; set; }
    public int? Weight { get; set; }
    public long? The5STemplateMainId { get; set; }
    public long? TheTemplateMainId { get; set; }
    public string? UseCondition { get; set; }
    public DateTime? LastCheckDate { get; set; }
    public DateTime? NextCheckDate { get; set; }
    public DateTime? LastRepairDate { get; set; }
    public string? AssetManager { get; set; }
    public string FacilitySign { get; set; } = string.Empty;
    public int? Continuous_WorkTime { get; set; }
    public int RunTime { get; set; }
    public int ElectrifyTime { get; set; }
    public int Continuous_Qty { get; set; }
    public int Status { get; set; }
    public long? InWarehouseUserId { get; set; }
    public DateTime? InWarehouseDate { get; set; }
    public DateTime? CreateDate { get; set; }
    public long? CreateUserId { get; set; }
    public string? TerminalAddress { get; set; }
    public string? FormulaIds { get; set; }
    public long? MonthTempId { get; set; }
    public long? SeasonTempId { get; set; }
    public long? HalfYearTempId { get; set; }
    public long? WeekTempId { get; set; }
    public long? YearTempId { get; set; }
    public DateTime? LastMonthMainTainDate { get; set; }
    public DateTime? LastYSeasonMainTainDate { get; set; }
    public DateTime? LastHalfYearMainTainDate { get; set; }
    public DateTime? LastYearMainTainDate { get; set; }
    public int Type { get; set; }
    public string Standard { get; set; } = string.Empty;
    public string Keeper { get; set; } = string.Empty;
    public long MonthPlanDay { get; set; }
    public int MonthWeek { get; set; }
    public string? Remark { get; set; }
    public DateTime? AcceptanceDate { get; set; }
    public string? NWXCode { get; set; }
    public int KeyFlag { get; set; }
    public decimal StandardYears { get; set; }
    public long EntityId { get; set; }
    public string? ManufactureNumber { get; set; }
    public string? EquipmentBodyNumber { get; set; }
    public string? MeasurementRange { get; set; }
    public string? Resolution { get; set; }
    public string? Accuracy { get; set; }
    public DateTime? CalibrationDate { get; set; }
    public string? CalibrationCycle { get; set; }
    public DateTime? CalibrationExpirationDate { get; set; }
    public int? CalibrationExpirationWarningDays { get; set; }
    public string? Custodian { get; set; }
    public string? ActualValue { get; set; }
}
