namespace RaylibNodeLibrary.DataModel;

public class TypeManager
{
    private Dictionary<int, DataType> typesById;
    private Dictionary<string, DataType> typesByName;

    public TypeManager()
    {
        typesById = new Dictionary<int, DataType>();
        typesByName = new Dictionary<string, DataType>();
    }

    public List<DataType> AllTypes => [.. typesById.Values];

    public DataType RegisterType(string name, DataCategory category, Type? cSharpType = null)
    {
        if (typesByName.ContainsKey(name))
            return typesByName[name];

        DataType newType = new DataType(name, category, cSharpType);
        typesById.Add(newType.Id, newType);
        typesByName.Add(name, newType);

        return newType;
    }

    public DataType? GetType(string name)
    {
        if (typesByName.TryGetValue(name, out DataType? type))
            return type;
        return null;
    }

    public DataType? GetType(int id)
    {
        if (typesById.TryGetValue(id, out DataType? type))
            return type;
        return null;
    }

    public void RegisterDefaultTypes()
    {
        DataType exec = RegisterType("Execution", DataCategory.Execution);
        DataType dataInt = RegisterType("Int", DataCategory.Data, typeof(int));
        DataType dataFloat = RegisterType("Float", DataCategory.Data, typeof(float));
        DataType dataNumber = RegisterType("Number", DataCategory.Data); // Abstract base for numbers
        DataType dataString = RegisterType("String", DataCategory.Data, typeof(string));
        DataType dataBool = RegisterType("Bool", DataCategory.Data, typeof(bool));

        // Int and Float can be assigned to Number
        dataInt.AddAssignableTo(dataNumber);
        dataFloat.AddAssignableTo(dataNumber);
        
        // Let's say Int can be assigned to Float implicitly
        dataInt.AddAssignableTo(dataFloat);
    }
}
