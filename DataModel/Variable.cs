namespace RaylibNodeLibrary.DataModel;

public class Variable : DataObject
{
    private string varName;
    private DataType varType;
    private object varValue;

    public string VarName { get => varName; }
    public DataType VarType { get => varType; }
    public object VarValue { get => varValue; set => varValue = value; }

    public Variable(string varName, DataType varType, object varValue) : base()
    {
        this.varName = varName;
        this.varType = varType;
        this.varValue = varValue;
    }

    public Variable(int id, string varName, DataType varType, object varValue) : base(id)
    {
        this.varName = varName;
        this.varType = varType;
        this.varValue = varValue;
    }

    public void SetName_Graph(string newName)
    {
        varName = newName;
    }

    public void ChangeType(DataType newType, object newDefaultValue)
    {
        varType = newType;
        varValue = newDefaultValue;
    }

    public override string ToString()
    {
        return $"{Id}: {varValue.GetType()} {varName} = {varValue}";
    }
}
