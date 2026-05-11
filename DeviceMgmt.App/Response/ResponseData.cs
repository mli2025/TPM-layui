namespace DeviceMgmt.App.Response;

public class ResponseData
{
    public int code { get; set; } = 200;
    public string msg { get; set; } = string.Empty;
    public object? data { get; set; }
}

public class TableData
{
    public int code { get; set; } = 200;
    public string msg { get; set; } = string.Empty;
    public int count { get; set; }
    public object? data { get; set; }
}

public class LoginResult
{
    public int code { get; set; } = 200;
    public string msg { get; set; } = string.Empty;
    public string? Token { get; set; }
    public bool success { get; set; }
}
