using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("WMS_BarCodeInfo_Spares_Sub")]
public class WMS_BarCodeInfo_Spares_Sub : Entity
{
    public string HSparesBarCode { get; set; } = string.Empty;
    public string BarCode { get; set; } = string.Empty;
    public int Status { get; set; }
}
