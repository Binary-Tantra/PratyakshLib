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

        horizontalPadding = 10;
    }

    public List<(UIElementType type, UIElementDescription desc)> GetInspectorUIDescriptors(Variable v)
    {
        DataType varType = v.VarType;
        List<(UIElementType, UIElementDescription)> list =
            [
                (UIElementType.BindableInputField_String, new BindableInputFieldStringDesc("Var Name", v.VarNameBindable))
            ];

        if (varType.Name == "Bool" && v.BoolValueBindable != null)
        {
            list.Add((UIElementType.BindableToggle, new BindableToggleDesc("Value", v.BoolValueBindable)));
        }
        else if (varType.Name == "Int" && v.IntValueBindable != null)
        {
            list.Add((UIElementType.BindableInputField_Int, new BindableInputFieldIntDesc("0", v.IntValueBindable)));
        }
        else if (varType.Name == "Float" && v.FloatValueBindable != null)
        {
            list.Add((UIElementType.BindableInputField_Float, new BindableInputFieldFloatDesc("0.0", v.FloatValueBindable)));
        }
        else if (varType.Name == "String" && v.StringValueBindable != null)
        {
            list.Add((UIElementType.BindableInputField_String, new BindableInputFieldStringDesc("Text", v.StringValueBindable)));
        }
        else if (varType.Name == "Number" && v.FloatValueBindable != null)
        {
            list.Add((UIElementType.BindableInputField_Float, new BindableInputFieldFloatDesc("0.0", v.FloatValueBindable)));
        }

        return list;
    }

    public override void OnDrawLayout()
    {
        (verticalBgOffset, verticalDrawStopOffset) = layout.DrawParentBG(Raylib.Fade(Color.Gray, 0.65f), 0.1f, "Inspector", Raylib.Fade(Color.DarkGray, 0.65f), Raylib.Fade(Color.White, 0.7f), negativeDrawStopY: -50);

        if (NodeEditorEngine.CurrentlySelectedObjectId != null && NodeEditorEngine.Graph.Variables.TryGetValue((int)NodeEditorEngine.CurrentlySelectedObjectId, out Variable? v))
        {
            var descriptors = GetInspectorUIDescriptors(v);

            // Render Name (Bindable InputField)
            layout.BeginHorizontal(10);
            {
                layout.Text("Name:", 15, Color.White);
                layout.BindableInputFieldString(v.Id + 2000000, "Var Name", v.VarNameBindable, RemainingWidth, 25);
            }
            layout.EndHorizontal(25);

            layout.AddSpace(5);

            // Render Type (Button selector)
            layout.BeginHorizontal(10);
            {
                layout.Text("Type:", 15, Color.White);

                List<DataType> dataTypes = [.. NodeEditorEngine.Graph.Types.AllTypes.Where(t => t.Category == DataCategory.Data)];
                layout.Button(v.Id + 3000000, v.VarType.Name, RemainingWidth, 25, (btn) =>
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
                if (item.type == UIElementType.BindableInputField_String && item.desc is BindableInputFieldStringDesc strDesc && strDesc.dataModel == v.VarNameBindable)
                {
                    // Already drew VarName
                    continue;
                }

                layout.BeginHorizontal(10);
                {
                    layout.Text("Value:", 15, Color.White);

                    if (item.type == UIElementType.BindableToggle && item.desc is BindableToggleDesc togDesc)
                    {
                        layout.BindableToggle(v.Id + 4000000, togDesc.dataModel, 50, 20);
                    }
                    else if (item.type == UIElementType.BindableInputField_Int && item.desc is BindableInputFieldIntDesc intDesc)
                    {
                        layout.BindableInputFieldInt(v.Id + 5000000, "0", intDesc.dataModel, RemainingWidth, 25);
                    }
                    else if (item.type == UIElementType.BindableInputField_Float && item.desc is BindableInputFieldFloatDesc fltDesc)
                    {
                        layout.BindableInputFieldFloat(v.Id + 6000000, "0.0", fltDesc.dataModel, RemainingWidth, 25);
                    }
                    else if (item.type == UIElementType.BindableInputField_String && item.desc is BindableInputFieldStringDesc valStrDesc)
                    {
                        layout.BindableInputFieldString(v.Id + 7000000, "Text", valStrDesc.dataModel, RemainingWidth, 25);
                    }
                }
                layout.EndHorizontal(25);
            }
        }
    }
}
