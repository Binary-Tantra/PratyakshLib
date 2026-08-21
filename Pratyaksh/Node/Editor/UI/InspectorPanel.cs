using Raylib_cs;
using Pratyaksh.Node.Core.DataModel;
using Pratyaksh.Core;
using Pratyaksh.UI;

namespace Pratyaksh.Node.Editor.UI;

public class InspectorPanel : UILayoutBase
{
    private Action<int, string> onRenameVariable;
    private Action<int, DataType> onChangeVariableType;
    private Action<int, object> onChangeVariableValue;
    private Action<int, int, List<(string, object)>, Action<object>> requestSearchMenu;

    protected override string PanelName => "InspectorPanel";

    public InspectorPanel(int posX, int posY, Action<int, string> onRenameVariable, Action<int, DataType> onChangeVariableType, Action<int, object> onChangeVariableValue, Action<int, int, List<(string, object)>, Action<object>> requestSearchMenu, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, 200, 300, parent, parentBasis)
    {
        this.onRenameVariable = onRenameVariable;
        this.onChangeVariableType = onChangeVariableType;
        this.onChangeVariableValue = onChangeVariableValue;
        this.requestSearchMenu = requestSearchMenu;
    }

    public List<(UIElementType type, UIElementDescription desc)> GetInspectorUIDescriptors(Variable v)
    {
        DataType varType = v.VarType;
        List<(UIElementType, UIElementDescription)> list =
            [
                (UIElementType.InputField, new InputFieldDesc("Var Name", v.VarNameBindable))
            ];

        if (varType.Name == "Bool" && v.BoolValueBindable != null)
        {
            list.Add((UIElementType.Toggle, new ToggleDesc("Value", v.BoolValueBindable)));
        }
        else if (varType.Name == "Int" && v.IntValueBindable != null)
        {
            list.Add((UIElementType.InputField, new InputFieldDesc("0", v.IntValueBindable)));
        }
        else if (varType.Name == "Float" && v.FloatValueBindable != null)
        {
            list.Add((UIElementType.InputField, new InputFieldDesc("0.0", v.FloatValueBindable)));
        }
        else if (varType.Name == "String" && v.StringValueBindable != null)
        {
            list.Add((UIElementType.InputField, new InputFieldDesc("Text", v.StringValueBindable)));
        }

        return list;
    }

    public override void OnDrawLayout()
    {
        layout.SectionEx("Inspector", Width, Height - 50, Raylib.Fade(Color.DarkGray, 0.65f), Raylib.Fade(Color.Gray, 0.65f), Raylib.Fade(Color.White, 0.7f), 0.1f, false);

        if (NodeEditorEngine.CurrentlySelectedObjectId != null && NodeEditorEngine.Graph.Variables.TryGetValue((int)NodeEditorEngine.CurrentlySelectedObjectId, out Variable? v))
        {
            var descriptors = GetInspectorUIDescriptors(v);

            layout.AddSpace(10);

            layout.BeginHorizontal(10);
            {
                layout.AddSpace(5);
                layout.BeginVerticalEx(5, (int)Position.Y + 35);
                {
                    // Render Name (Bindable InputField)
                    layout.BeginHorizontal(0);
                    {
                        layout.Text("Name:", Color.White);
                        layout.AddSpace(40);
                        layout.BindableInputFieldString(v.Id + 2000000, "Var Name", v.VarNameBindable, Width - 60, 25);
                    }
                    layout.EndHorizontal(25);

                    layout.AddSpace(5);

                    // Render Type (Button selector)
                    layout.BeginHorizontal(0);
                    {
                        layout.Text("Type:", Color.White);
                        layout.AddSpace(40);

                        List<DataType> dataTypes = [.. NodeEditorEngine.Graph.Types.AllTypes.Where(t => t.Category == DataCategory.Data)];
                        layout.Button(v.Id + 3000000, v.VarType.Name, Width - 60, 25, (btn) =>
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

                    // Render Value generic bindables from descriptors
                    foreach (var item in descriptors)
                    {
                        if (item.desc is InputFieldDesc ifDesc && ifDesc.StringDataModel == v.VarNameBindable)
                        {
                            // Already drew VarName
                            continue;
                        }

                        layout.BeginHorizontal(0);
                        {
                            layout.Text("Value:", Color.White);
                            layout.AddSpace(40);

                            if (item.desc is ToggleDesc togDesc)
                            {
                                togDesc.Width = 50;
                                togDesc.Height = 20;
                                layout.Toggle(v.Id + 4000000, togDesc);
                            }
                            else if (item.desc is InputFieldDesc inputDesc)
                            {
                                inputDesc.Width = Width - 60;
                                inputDesc.Height = 25;
                                layout.InputField(v.Id + 5000000, inputDesc);
                            }
                        }
                        layout.EndHorizontal(25);
                        layout.AddSpace(5);
                    }
                }
                layout.EndVertical(Width - 20);
            }
            layout.EndHorizontal(Height - 50);
        }
    }
}
