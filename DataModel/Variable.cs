namespace RaylibNodeLibrary.DataModel;

public class Variable : DataObject
{
    private string varName;
    private Type varType;
    private object varValue;

    public string VarName { get => varName; }
    public Type VarType { get => varType; }
    public object VarValue { get => varValue; }

    public Variable(string varName, Type varType, object varValue) : base()
    {
        this.varName = varName;
        this.varType = varType;
        this.varValue = varValue;
    }

    public void SetName_Graph(string newName)
    {
        varName = newName;
    }

    public override string ToString()
    {
        return $"{Id}: {varValue.GetType()} {varName} = {varValue}";
    }
}
