using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class ChildLayout : UILayoutBase
{
    private List<(UIElementType elemType, UIElementDescription elemDesc)> uiElements;
    private readonly List<List<(int id, object? payload)>> ids; // The third thing (object) is a payload for things like dropdown which need an extra thing (in its case, its selected option idx).

    private int maxWidthCoverage;

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

                for (int j = 0; j < gd.uiElements.Count; j++)
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
                    layout.Text(textDesc.text, textDesc.color);
                    break;
                case UIElementType.Button:
                    ButtonDesc buttonDesc = (ButtonDesc)desc;
                    layout.Button(id, buttonDesc.text, buttonDesc.width ?? maxWidthCoverage, buttonDesc.height ?? 25, buttonDesc.onClick, id);
                    break;
                case UIElementType.InputField:
                    InputFieldDesc ifDesc = (InputFieldDesc)desc;
                    payload = ids[i][j].payload ?? ifDesc.text;
                    layout.InputField(id, ifDesc.placeholderText, (string)payload, ifDesc.width ?? maxWidthCoverage, ifDesc.height ?? 25, (ifld) =>
                    {
                        ids[i][j] = (ids[i][j].id, ifld.InputFieldText);
                        ifDesc.onTextChanged?.Invoke(ifld);
                    }, ifDesc.onFocusEnd);
                    break;
                case UIElementType.Selectable:
                    SelectableDesc selDesc = (SelectableDesc)desc;
                    payload = ids[i][j].payload ?? selDesc.startingSelected;
                    layout.Selectable(id, (bool)payload, selDesc.text, selDesc.width ?? maxWidthCoverage, selDesc.height ?? 25, (sel) =>
                    {
                        ids[i][j] = (ids[i][j].id, sel.IsSelected);
                        selDesc.onSelect?.Invoke(sel);
                    }, id);
                    break;
                case UIElementType.Toggle:
                    ToggleDesc togDesc = (ToggleDesc)desc;
                    payload = ids[i][j].payload ?? togDesc.startingState;
                    layout.Toggle(id, (bool)payload, togDesc.width ?? maxWidthCoverage, togDesc.height ?? 20, (tog) =>
                    {
                        ids[i][j] = (ids[i][j].id, tog.Value);
                        togDesc.onToggle?.Invoke(tog);
                    }, id);
                    break;
                case UIElementType.Dropdown:
                    DropdownDesc ddDesc = (DropdownDesc)desc;
                    payload = ids[i][j].payload ?? ddDesc.selectedIndex;
                    layout.Dropdown(id, ddDesc.options, (int)payload, ddDesc.width ?? maxWidthCoverage, ddDesc.height ?? 25, (dd) =>
                    {
                        ids[i][j] = (ids[i][j].id, dd.SelectedIndex);
                        ddDesc.onSelectionChanged?.Invoke(dd);
                    }, id);
                    break;
                case UIElementType.BindableToggle:
                    BindableToggleDesc bTogDesc = (BindableToggleDesc)desc;
                    layout.BindableToggle(id, bTogDesc.dataModel, bTogDesc.width ?? maxWidthCoverage, bTogDesc.height ?? 20);
                    break;
                case UIElementType.BindableInputField_String:
                    BindableInputFieldStringDesc bIfStrDesc = (BindableInputFieldStringDesc)desc;
                    layout.BindableInputFieldString(id, bIfStrDesc.placeholderText, bIfStrDesc.dataModel, bIfStrDesc.width ?? maxWidthCoverage, bIfStrDesc.height ?? 25);
                    break;
                case UIElementType.BindableInputField_Int:
                    BindableInputFieldIntDesc bIfIntDesc = (BindableInputFieldIntDesc)desc;
                    layout.BindableInputFieldInt(id, bIfIntDesc.placeholderText, bIfIntDesc.dataModel, bIfIntDesc.width ?? maxWidthCoverage, bIfIntDesc.height ?? 25);
                    break;
                case UIElementType.BindableInputField_Float:
                    BindableInputFieldFloatDesc bIfFltDesc = (BindableInputFieldFloatDesc)desc;
                    layout.BindableInputFieldFloat(id, bIfFltDesc.placeholderText, bIfFltDesc.dataModel, bIfFltDesc.width ?? maxWidthCoverage, bIfFltDesc.height ?? 25);
                    break;
                case UIElementType.BindableSelectable:
                    BindableSelectableDesc bSelDesc = (BindableSelectableDesc)desc;
                    layout.BindableSelectable(id, bSelDesc.dataModel, bSelDesc.text, bSelDesc.width ?? maxWidthCoverage, bSelDesc.height ?? 25);
                    break;
                case UIElementType.BindableDropdown:
                    BindableDropdownDesc bDdDesc = (BindableDropdownDesc)desc;
                    layout.BindableDropdown(id, bDdDesc.options, bDdDesc.dataModel, bDdDesc.width ?? maxWidthCoverage, bDdDesc.height ?? 25);
                    break;
            }
        }

        for (int i = 0; i < uiElements.Count; i++)
        {
            if (uiElements[i].elemType == UIElementType.Group)
            {
                HorizontalGroupDesc gDesc = (HorizontalGroupDesc)uiElements[i].elemDesc;
                maxWidthCoverage = (int)((float)Width / gDesc.uiElements.Count);

                layout.BeginHorizontal(gDesc.spacing);
                {
                    for (int j = 0; j < gDesc.uiElements.Count; j++)
                    {
                        maxWidthCoverage = (int)((float)(Width - layout.CurrentWidth()) / (gDesc.uiElements.Count - j));

                        (int id, object? payload) = ids[i][j];
                        DrawAccType(id, gDesc.uiElements[j].elemType, gDesc.uiElements[j].elemDesc, i, j);
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
                        }
                        if (val != null) ids[i][j] = (ids[i][j].id, val);
                    }
                    idx++;
                }
            }
        }
    }
}
