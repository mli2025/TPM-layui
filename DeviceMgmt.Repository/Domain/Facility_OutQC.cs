using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("Facility_OutQC")]
public class Facility_OutQC : Entity
{
    public long FacilityId { get; set; }
    public DateTime OutDate { get; set; } = DateTime.Now;
    public long EmpId { get; set; }
    public long SupplierId { get; set; }
    public string InspectionAddress { get; set; } = string.Empty;
    public long? AcceptancePersonnel { get; set; }
    public DateTime? AcceptanceTime { get; set; }
    public string? AcceptanceDocuments { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
    public long CreateUserId { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public long LastUpdateUserId { get; set; }
    public DateTime LastUpdateDate { get; set; } = DateTime.Now;
}
