using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("WMS_BarCodeInfo_Spares")]
public class WMS_BarCodeInfo_Spares : Entity
{
    public string BarCode { get; set; } = string.Empty;
    public int Status { get; set; }
}
