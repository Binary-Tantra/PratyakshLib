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
            var descriptors = v.GetInspectorUIDescriptors();

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
                        layout.BindableInputFieldString(v.Id + 2000000, "Var Name", v.VarNameBindable, layoutWidth - 60, 25);
                    }
                    layout.EndHorizontal(25);

                    layout.AddSpace(5);

                    // Render Type (Button selector)
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

                    // Render Value generic bindables from descriptors
                    foreach (var item in descriptors)
                    {
                        if (item.type == UIElementType.BindableInputField_String && item.desc is BindableInputFieldStringDesc strDesc && strDesc.dataModel == v.VarNameBindable)
                        {
                            // Already drew VarName
                            continue;
                        }

                        layout.BeginHorizontal(0);
                        {
                            layout.Text("Value:", Color.White);
                            layout.AddSpace(40);

                            if (item.type == UIElementType.BindableToggle && item.desc is BindableToggleDesc togDesc)
                            {
                                layout.BindableToggle(v.Id + 4000000, togDesc.dataModel, togDesc.dataModel.Get() ? "True" : "False", 50, 20);
                            }
                            else if (item.type == UIElementType.BindableInputField_Int && item.desc is BindableInputFieldIntDesc intDesc)
                            {
                                layout.BindableInputFieldInt(v.Id + 5000000, "0", intDesc.dataModel, layoutWidth - 60, 25);
                            }
                            else if (item.type == UIElementType.BindableInputField_Float && item.desc is BindableInputFieldFloatDesc fltDesc)
                            {
                                layout.BindableInputFieldFloat(v.Id + 6000000, "0.0", fltDesc.dataModel, layoutWidth - 60, 25);
                            }
                            else if (item.type == UIElementType.BindableInputField_String && item.desc is BindableInputFieldStringDesc valStrDesc)
                            {
                                layout.BindableInputFieldString(v.Id + 7000000, "Text", valStrDesc.dataModel, layoutWidth - 60, 25);
                            }
                        }
                        layout.EndHorizontal(25);
                        layout.AddSpace(5);
                    }
                }
                layout.EndVertical(layoutWidth - 20);
            }
            layout.EndHorizontal(layoutHeight - 50);
        }
    }
}
