using Pratyaksh.Core;

namespace Pratyaksh.UI;

public class ChildLayout : UILayoutBase
{
    private List<(UIElementType elemType, UIElementDescription elemDesc)> uiElements;
    private readonly List<List<(int id, object? payload)>> ids; // The third thing (object) is a payload for things like dropdown which need an extra thing (in its case, its selected option idx).

    private int maxWidthCoverage;

    protected override string PanelName => parent?.GetType().Name + "ChildLayout";

    public ChildLayout(List<(UIElementType type, UIElementDescription desc)> uiElements, int posX, int posY, int layoutWidth, int layoutHeight, Drawable? parent, ParentBasis? parentBasis = null) : base(posX, posY, layoutWidth, layoutHeight, parent, parentBasis)
    {
        selfInteractable = false;
        this.uiElements = uiElements;

        mainVerticalSpacing = 5;

        ids = [];
        for (int i = 0; i < uiElements.Count; i++)
        {
            if (uiElements[i].type == UIElementType.Group)
            {
                HorizontalGroupDesc gd = (HorizontalGroupDesc)uiElements[i].desc;
                List<(int, object?)> gdIds = [];

                for (int j = 0; j < gd.UIElements.Count; j++)
                    gdIds.Add((IdGen.GetNewID(), null));

                ids.Add(gdIds);
            }
            else ids.Add([(IdGen.GetNewID(), null)]);
        }
    }

    public override void OnDrawLayout()
    {
        void DrawAccType(int id, UIElementType type, UIElementDescription desc, int i, int j)
        {
            object? payload = null;

            switch (type)
            {
                case UIElementType.Text:
                    TextDesc textDesc = (TextDesc)desc;
                    layout.Text(textDesc.Text, textDesc.Color);
                    break;
                case UIElementType.Button:
                    ButtonDesc buttonDesc = (ButtonDesc)desc;
                    layout.Button(id, buttonDesc.Text, buttonDesc.Width ?? maxWidthCoverage, buttonDesc.Height ?? 25, buttonDesc.OnClick, id);
                    break;
                case UIElementType.InputField:
                case UIElementType.BindableInputField_String:
                case UIElementType.BindableInputField_Int:
                case UIElementType.BindableInputField_Float:
                    InputFieldDesc ifDesc = (InputFieldDesc)desc;
                    if (ifDesc.IsBindable)
                    {
                        layout.InputField(id, ifDesc);
                    }
                    else
                    {
                        payload = ids[i][j].payload ?? ifDesc.Text;
                        layout.InputField(id, ifDesc.PlaceholderText, (string)payload, ifDesc.Width ?? maxWidthCoverage, ifDesc.Height ?? 25, (ifld) =>
                        {
                            ids[i][j] = (ids[i][j].id, ifld.InputFieldText);
                            ifDesc.OnTextChanged?.Invoke(ifld);
                        }, ifDesc.OnFocusEnd);
                    }
                    break;
                case UIElementType.Selectable:
                case UIElementType.BindableSelectable:
                    SelectableDesc selDesc = (SelectableDesc)desc;
                    if (selDesc.IsBindable)
                    {
                        layout.Selectable(id, selDesc);
                    }
                    else
                    {
                        payload = ids[i][j].payload ?? selDesc.StartingSelected;
                        layout.Selectable(id, (bool)payload, selDesc.Text, selDesc.Width ?? maxWidthCoverage, selDesc.Height ?? 25, (sel) =>
                        {
                            ids[i][j] = (ids[i][j].id, sel.IsSelected);
                            selDesc.OnSelect?.Invoke(sel);
                        }, id);
                    }
                    break;
                case UIElementType.Toggle:
                case UIElementType.BindableToggle:
                    ToggleDesc togDesc = (ToggleDesc)desc;
                    if (togDesc.IsBindable)
                    {
                        layout.Toggle(id, togDesc);
                    }
                    else
                    {
                        payload = ids[i][j].payload ?? togDesc.StartingState;
                        layout.Toggle(id, (bool)payload, togDesc.Width ?? maxWidthCoverage, togDesc.Height ?? 20, (tog) =>
                        {
                            ids[i][j] = (ids[i][j].id, tog.Value);
                            togDesc.OnToggle?.Invoke(tog);
                        }, id);
                    }
                    break;
                case UIElementType.Dropdown:
                case UIElementType.BindableDropdown:
                    DropdownDesc ddDesc = (DropdownDesc)desc;
                    if (ddDesc.IsBindable)
                    {
                        layout.Dropdown(id, ddDesc);
                    }
                    else
                    {
                        payload = ids[i][j].payload ?? ddDesc.SelectedIndex;
                        layout.Dropdown(id, ddDesc.Options, (int)payload, ddDesc.Width ?? maxWidthCoverage, ddDesc.Height ?? 25, (dd) =>
                        {
                            ids[i][j] = (ids[i][j].id, dd.SelectedIndex);
                            ddDesc.OnSelectionChanged?.Invoke(dd);
                        }, id);
                    }
                    break;
                case UIElementType.Slider:
                case UIElementType.BindableSlider:
                    SliderDesc slDesc = (SliderDesc)desc;
                    if (slDesc.IsBindable)
                    {
                        layout.Slider(id, slDesc);
                    }
                    else
                    {
                        payload = ids[i][j].payload ?? slDesc.Value;
                        layout.Slider(id, Convert.ToSingle(payload), slDesc.MinValue, slDesc.MaxValue, slDesc.Width ?? maxWidthCoverage, slDesc.Height ?? 20, (sl) =>
                        {
                            ids[i][j] = (ids[i][j].id, sl.Value);
                            slDesc.OnValueChanged?.Invoke(sl);
                        }, id, slDesc.ShowValue, slDesc.Format, slDesc.Step);
                    }
                    break;
            }
        }

        for (int i = 0; i < uiElements.Count; i++)
        {
            if (uiElements[i].elemType == UIElementType.Group)
            {
                HorizontalGroupDesc gDesc = (HorizontalGroupDesc)uiElements[i].elemDesc;
                maxWidthCoverage = (int)((float)Width / gDesc.UIElements.Count);

                layout.BeginHorizontal(gDesc.Spacing);
                {
                    for (int j = 0; j < gDesc.UIElements.Count; j++)
                    {
                        maxWidthCoverage = (int)((float)(Width - layout.CurrentWidth()) / (gDesc.UIElements.Count - j));

                        (int id, object? payload) = ids[i][j];
                        DrawAccType(id, gDesc.UIElements[j].elemType, gDesc.UIElements[j].elemDesc, i, j);
                        ids[i][j] = (id, payload);
                    }
                }
                layout.EndHorizontal(25);
            }
            else
            {
                maxWidthCoverage = Width;
                DrawAccType(ids[i][0].id, uiElements[i].elemType, uiElements[i].elemDesc, i, 0);
            }
        }
    }

    public void ChangeUIElement(int elementIdx, UIElementDescription desc)
    {
        Type uie = uiElements[elementIdx].elemDesc.GetType();
        Type rie = desc.GetType();

        if (uie != rie)
        {
            Console.WriteLine($"Error: The UIElement idx {elementIdx} is of type {uie} and not of the requested type {rie}.");
            return;
        }

        uiElements[elementIdx] = (uiElements[elementIdx].elemType, desc);
    }

    public List<object?> GetUIStatePayloads()
    {
        List<object?> payloads = new();
        for (int i = 0; i < ids.Count; i++)
        {
            for (int j = 0; j < ids[i].Count; j++)
            {
                payloads.Add(ids[i][j].payload);
            }
        }
        return payloads;
    }

    public void SetUIStatePayloads(List<System.Text.Json.JsonElement?> savedPayloads)
    {
        if (savedPayloads == null) return;
        int idx = 0;
        for (int i = 0; i < ids.Count; i++)
        {
            for (int j = 0; j < ids[i].Count; j++)
            {
                if (idx < savedPayloads.Count)
                {
                    var elem = savedPayloads[idx];
                    if (elem.HasValue)
                    {
                        object? val = null;
                        var k = elem.Value.ValueKind;
                        if (k == System.Text.Json.JsonValueKind.String) val = elem.Value.GetString();
                        else if (k == System.Text.Json.JsonValueKind.True) val = true;
                        else if (k == System.Text.Json.JsonValueKind.False) val = false;
                        else if (k == System.Text.Json.JsonValueKind.Number)
                        {
                            if (elem.Value.TryGetInt32(out int intVal)) val = intVal;
                            else if (elem.Value.TryGetSingle(out float fltVal)) val = fltVal;
                            else if (elem.Value.TryGetDouble(out double dblVal)) val = (float)dblVal;
                        }
                        if (val != null) ids[i][j] = (ids[i][j].id, val);
                    }
                    idx++;
                }
            }
        }
    }
}
