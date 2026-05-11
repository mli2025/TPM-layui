namespace DeviceMgmt.App.Request;

public class QuerySelect2Req
{
    public string? q { get; set; }
    public int page { get; set; } = 1;
    public string? table { get; set; }
    public string? fit { get; set; }
    public string? valueColumn { get; set; }
    public string? displayColumn { get; set; }
    public string? optional { get; set; }
    public string? name { get; set; }
    public string? selectedId { get; set; }
}
