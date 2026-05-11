namespace DeviceMgmt.App.Request;

public class PageReq
{
    public int page { get; set; } = 1;
    public int limit { get; set; } = 10;
    public string? key { get; set; }
    public string? query { get; set; }
    public string? sfield { get; set; }
    public string? sorder { get; set; }
    public List<searchParam>? searchParam { get; set; }
}

public class searchParam
{
    public string field { get; set; } = string.Empty;
    public string conditional { get; set; } = "like";
    public string value { get; set; } = string.Empty;
}
