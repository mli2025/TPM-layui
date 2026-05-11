namespace DeviceMgmt.Repository.Core;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class TableAttribute : Attribute
{
    public string Name { get; }
    public string Schema { get; }
    public TableAttribute(string name, string schema = "dbo")
    {
        Name = name;
        Schema = schema;
    }
}
