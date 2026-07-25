using Raylib_cs;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class InspectorPanel : UILayoutBase
{
    private Action<int, string> onRenameVariable;
    private Action<int, DataType> onChangeVariableType;
    private Action<int, object> onChangeVariableValue;
    private Action<int, int, List<(string, object)>, Action<object>> requestSearchMenu;

    public InspectorPanel(int posX, int posY, Action<int, string> onRenameVariable, Action<int, DataType> onChangeVariableType, Action<int, object> onChangeVariableValue, Action<int, int, List<(string, object)>, Action<object>> requestSearchMenu, Drawable? parent = null) : base(posX, posY, 200, 300, parent)
    {
        this.onRenameVariable = onRenameVariable;
        this.onChangeVariableType = onChangeVariableType;
        this.onChangeVariableValue = onChangeVariableValue;
        this.requestSearchMenu = requestSearchMenu;
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
                layout.BeginVerticalEx(5, (int)Position.Y + 35);
                {
                    layout.BeginHorizontal(0);
                    {
                        layout.Text("Name:", Color.White);
                        layout.AddSpace(40);
                        layout.InputField(v.Id + 2000000, "Var Name", v.VarName, layoutWidth - 60, 25, null, (input) =>
                        {
                            onRenameVariable?.Invoke(v.Id, input.InputFieldText);
                        });
                    }
                    layout.EndHorizontal(25);

                    layout.AddSpace(5);

                    layout.BeginHorizontal(0);
                    {
                        layout.Text("Type:", Color.White);

                        layout.AddSpace(40);

                        List<DataType> dataTypes = [.. Engine.Graph.Types.AllTypes.Where(t => t.Category == DataCategory.Data)];
                        layout.Button(v.Id + 3000000, v.VarType.Name, layoutWidth - 60, 25, (btn) =>
                        {
                            System.Numerics.Vector2 mp = Raylib.GetMousePosition();
                            List<(string name, object payload)> typeItems = [.. dataTypes.Select(dt => (dt.Name, (object)dt))];

                            requestSearchMenu?.Invoke((int)mp.X, (int)mp.Y, typeItems, (payload) =>
                            {
                                onChangeVariableType?.Invoke(v.Id, (DataType)payload);
                            });

                        }, v.Id);
                    }
                    layout.EndHorizontal(25);

                    layout.AddSpace(5);

                    layout.BeginHorizontal(0);
                    {
                        layout.Text("Value:", Color.White);

                        layout.AddSpace(40);

                        if (v.VarType.Name == "Bool")
                        {
                            bool bVal = (bool)v.VarValue;
                            layout.Toggle(v.Id + 4000000, bVal, bVal ? "True" : "False", 50, 20, (toggle) =>
                            {
                                onChangeVariableValue?.Invoke(v.Id, toggle.IsOn);
                            }, null);
                        }
                        else if (v.VarType.Name == "Int")
                        {
                            layout.InputField(v.Id + 5000000, "0", v.VarValue.ToString() ?? "0", layoutWidth - 60, 25, (input) =>
                            {
                                if (int.TryParse(input.InputFieldText, out int result))
                                    onChangeVariableValue?.Invoke(v.Id, result);
                            }, (input) =>
                            {
                                if (int.TryParse(input.InputFieldText, out int result))
                                    onChangeVariableValue?.Invoke(v.Id, result);
                                else
                                    input.InputFieldText = v.VarValue.ToString() ?? "0";
                            });
                        }
                        else if (v.VarType.Name == "Float" || v.VarType.Name == "Number")
                        {
                            layout.InputField(v.Id + 6000000, "0.0", v.VarValue.ToString() ?? "0.0", layoutWidth - 60, 25, (input) =>
                            {
                                if (float.TryParse(input.InputFieldText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
                                    onChangeVariableValue?.Invoke(v.Id, result);
                                else if (float.TryParse(input.InputFieldText, out float res2))
                                    onChangeVariableValue?.Invoke(v.Id, res2);
                            }, (input) =>
                            {
                                if (float.TryParse(input.InputFieldText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
                                    onChangeVariableValue?.Invoke(v.Id, result);
                                else if (float.TryParse(input.InputFieldText, out float res2))
                                    onChangeVariableValue?.Invoke(v.Id, res2);
                                else
                                    input.InputFieldText = v.VarValue.ToString() ?? "0.0";
                            });
                        }
                        else if (v.VarType.Name == "String")
                        {
                            layout.InputField(v.Id + 7000000, "Text", v.VarValue.ToString() ?? "", layoutWidth - 60, 25, (input) =>
                            {
                                onChangeVariableValue?.Invoke(v.Id, input.InputFieldText);
                            }, (input) =>
                            {
                                onChangeVariableValue?.Invoke(v.Id, input.InputFieldText);
                            });
                        }
                    }
                    layout.EndHorizontal(25);
                }
                layout.EndVertical(layoutWidth - 20);
            }
            layout.EndHorizontal(layoutHeight - 50);
        }
    }
}
