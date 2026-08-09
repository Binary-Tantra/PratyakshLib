using Pratyaksh.Core.DataBinding;
using RaylibNodeLibrary.UI;

namespace RaylibNodeLibrary.DataModel;

public class Variable : DataObject
{
    private DataType varType;

    public BindableString VarNameBindable { get; }
    public BindableBool? BoolValueBindable { get; private set; }
    public BindableInt? IntValueBindable { get; private set; }
    public BindableFloat? FloatValueBindable { get; private set; }
    public BindableString? StringValueBindable { get; private set; }

    public string VarName
    {
        get => VarNameBindable.Get();
        set => VarNameBindable.Set(value, true);
    }

    public DataType VarType => varType;

    public object VarValue
    {
        get
        {
            if (varType.Name == "Bool" && BoolValueBindable != null) return BoolValueBindable.Get();
            if (varType.Name == "Int" && IntValueBindable != null) return IntValueBindable.Get();
            if (varType.Name == "Float" && FloatValueBindable != null) return FloatValueBindable.Get();
            if (varType.Name == "String" && StringValueBindable != null) return StringValueBindable.Get();
            return 0;
        }
        set
        {
            SetTypedValue(value);
        }
    }

    public Variable(string varName, DataType varType, object varValue) : base()
    {
        VarNameBindable = new BindableString(varName);
        this.varType = varType;
        InitTypedValue(varValue);
    }

    public Variable(int id, string varName, DataType varType, object varValue) : base(id)
    {
        VarNameBindable = new BindableString(varName);
        this.varType = varType;
        InitTypedValue(varValue);
    }

    private void InitTypedValue(object val)
    {
        if (varType.Name == "Bool")
            BoolValueBindable = new BindableBool(val is bool b ? b : false);
        else if (varType.Name == "Int")
            IntValueBindable = new BindableInt(val is int i ? i : (val is long l ? (int)l : 0));
        else if (varType.Name == "Float")
            FloatValueBindable = new BindableFloat(val is float f ? f : (val is double d ? (float)d : 0f));
        else if (varType.Name == "String")
            StringValueBindable = new BindableString(val?.ToString() ?? "");
    }

    private void SetTypedValue(object val)
    {
        if (varType.Name == "Bool" && BoolValueBindable != null)
            BoolValueBindable.Set(val is bool b ? b : false, true);
        else if (varType.Name == "Int" && IntValueBindable != null)
            IntValueBindable.Set(val is int i ? i : (val is long l ? (int)l : 0), true);
        else if (varType.Name == "Float" && FloatValueBindable != null)
            FloatValueBindable.Set(val is float f ? f : (val is double d ? (float)d : 0f), true);
        else if (varType.Name == "String" && StringValueBindable != null)
            StringValueBindable.Set(val?.ToString() ?? "", true);
    }

    public void SetName_Graph(string newName)
    {
        VarNameBindable.Set(newName, true);
    }

    public void ChangeType(DataType newType, object newDefaultValue)
    {
        varType = newType;
        InitTypedValue(newDefaultValue);
    }

    public List<(UIElementType type, UIElementDescription desc)> GetInspectorUIDescriptors()
    {
        var list = new List<(UIElementType, UIElementDescription)>();

        list.Add((UIElementType.BindableInputField_String, new BindableInputFieldStringDesc("Var Name", VarNameBindable)));

        if (varType.Name == "Bool" && BoolValueBindable != null)
        {
            list.Add((UIElementType.BindableToggle, new BindableToggleDesc("Value", BoolValueBindable)));
        }
        else if (varType.Name == "Int" && IntValueBindable != null)
        {
            list.Add((UIElementType.BindableInputField_Int, new BindableInputFieldIntDesc("0", IntValueBindable)));
        }
        else if (varType.Name == "Float" && FloatValueBindable != null)
        {
            list.Add((UIElementType.BindableInputField_Float, new BindableInputFieldFloatDesc("0.0", FloatValueBindable)));
        }
        else if (varType.Name == "String" && StringValueBindable != null)
        {
            list.Add((UIElementType.BindableInputField_String, new BindableInputFieldStringDesc("Text", StringValueBindable)));
        }

        return list;
    }

    public override string ToString()
    {
        return $"{Id}: {varType.Name} {VarName} = {VarValue}";
    }
}
