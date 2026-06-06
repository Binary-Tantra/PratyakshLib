namespace RaylibNodeLibrary.DataModel;

public class Variable : DataObject
{
    private string varName;
    private object varValue;

    public string VarName { get => varName; }

    public Variable(string varName, object varValue) : base()
    {
        this.varName = varName;
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
