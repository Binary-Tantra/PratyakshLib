using Raylib_cs;
using RaylibNodeLibrary.DataModel;
using System.Linq;

namespace RaylibNodeLibrary.UI;

public class InspectorPanel : UILayoutBase
{
    private Action<int, string> onRenameVariable;
    private Action<int, DataType> onChangeVariableType;
    private Action<int, object> onChangeVariableValue;

    public InspectorPanel(int posX, int posY, Action<int, string> onRenameVariable, Action<int, DataType> onChangeVariableType, Action<int, object> onChangeVariableValue, Drawable? parent = null) : base(posX, posY, 200, 300, parent)
    {
        this.onRenameVariable = onRenameVariable;
        this.onChangeVariableType = onChangeVariableType;
        this.onChangeVariableValue = onChangeVariableValue;
    }

    public override void OnDrawLayout()
    {
        layout.SectionEx("Inspector", layoutWidth, layoutHeight - 50, Raylib.Fade(Color.DarkGray, 0.65f), Raylib.Fade(Color.Gray, 0.65f), Raylib.Fade(Color.White, 0.7f), 0.1f, false);

        if (Engine.CurrentlySelectedObjectId != null && Engine.Graph.Variables.TryGetValue((int)Engine.CurrentlySelectedObjectId, out Variable? v))
        {
            layout.AddSpace(10);
            layout.BeginHorizontal(10);
            {
                layout.AddSpace(5);
                layout.BeginVertical(5);
                {
                    layout.Text("Name:", Color.White);
                    layout.InputField(v.Id + 2000000, "Var Name", v.VarName, layoutWidth - 20, 25, null, (input) =>
                    {
                        onRenameVariable?.Invoke(v.Id, input.InputFieldText);
                    });

                    layout.AddSpace(5);
                    layout.Text("Type:", Color.White);
                    
                    List<DataType> dataTypes = Engine.Graph.Types.AllTypes.Where(t => t.Category == DataCategory.Data).ToList();
                    string[] dataTypeNames = dataTypes.Select(t => t.Name).ToArray();
                    int currentTypeIndex = dataTypes.FindIndex(t => t.Id == v.VarType.Id);
                    if (currentTypeIndex == -1) currentTypeIndex = 0;

                    layout.Dropdown(v.Id + 3000000, dataTypeNames, currentTypeIndex, layoutWidth - 20, 25, (dd, newIndex) =>
                    {
                        onChangeVariableType?.Invoke(v.Id, dataTypes[newIndex]);
                    }, v.Id);

                    layout.AddSpace(5);
                    layout.Text("Value:", Color.White);

                    if (v.VarType.Name == "Bool")
                    {
                        bool bVal = (bool)v.VarValue;
                        layout.Toggle(v.Id + 4000000, bVal, bVal ? "True" : "False", layoutWidth - 20, 20, (toggle) =>
                        {
                            onChangeVariableValue?.Invoke(v.Id, toggle.IsOn);
                        }, null);
                    }
                    else if (v.VarType.Name == "Int")
                    {
                        layout.InputField(v.Id + 5000000, "0", v.VarValue.ToString() ?? "0", layoutWidth - 20, 25, null, (input) =>
                        {
                            if (int.TryParse(input.InputFieldText, out int result))
                                onChangeVariableValue?.Invoke(v.Id, result);
                            else input.Text = v.VarValue.ToString() ?? "0";
                        });
                    }
                    else if (v.VarType.Name == "Float" || v.VarType.Name == "Number")
                    {
                        layout.InputField(v.Id + 6000000, "0.0", v.VarValue.ToString() ?? "0.0", layoutWidth - 20, 25, null, (input) =>
                        {
                            if (float.TryParse(input.InputFieldText, out float result))
                                onChangeVariableValue?.Invoke(v.Id, result);
                            else input.Text = v.VarValue.ToString() ?? "0.0";
                        });
                    }
                    else if (v.VarType.Name == "String")
                    {
                        layout.InputField(v.Id + 7000000, "Text", v.VarValue.ToString() ?? "", layoutWidth - 20, 25, null, (input) =>
                        {
                            onChangeVariableValue?.Invoke(v.Id, input.InputFieldText);
                        });
                    }
                }
                layout.EndVertical(layoutWidth - 20);
            }
            layout.EndHorizontal(layoutHeight - 50);
        }
    }
}
