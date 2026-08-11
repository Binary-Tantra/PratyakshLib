namespace Pratyaksh.Node.Core.DataModel;

[Flags]
public enum DataCategory
{
    None = 0,
    Execution = 1,
    Data = 2,
    Collection = 4
}

public class DataType : DataObject
{
    public string Name { get; private set; }
    public Type? CSharpType { get; private set; }
    public DataCategory Category { get; private set; }

    private HashSet<int> assignableToTypeIds;

    public DataType(string name, DataCategory category, Type? cSharpType = null) : base()
    {
        Name = name;
        Category = category;
        CSharpType = cSharpType;
        assignableToTypeIds = new HashSet<int>();
    }

    public void AddAssignableTo(DataType targetType)
    {
        assignableToTypeIds.Add(targetType.Id);
    }

    public bool CanAssignTo(DataType targetType)
    {
        return targetType.Id == this.Id || assignableToTypeIds.Contains(targetType.Id);
    }
}
