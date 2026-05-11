namespace Infrastructure;

public class Filter
{
    public string field { get; set; } = string.Empty;
    public string conditional { get; set; } = "like";
    public object? Value { get; set; }
}
