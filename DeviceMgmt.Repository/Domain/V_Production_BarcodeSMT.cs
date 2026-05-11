using DeviceMgmt.Repository.Core;

namespace DeviceMgmt.Repository.Domain;

[Table("V_Production_BarcodeSMT")]
public class V_Production_BarcodeSMT : Entity
{
    public DateTime Date { get; set; } = DateTime.Now;
    public string BarCode { get; set; } = string.Empty;
    public string ProcedureNo { get; set; } = string.Empty;
    public string Num { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
