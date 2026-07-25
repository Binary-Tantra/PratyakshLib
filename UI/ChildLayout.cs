using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class ChildLayout : UILayoutBase
{
    private List<(UIElementType elemType, UIElementDescription elemDesc)> uiElements;
    private readonly List<List<(int id, object? payload)>> ids; // The third thing (object) is a payload for things like dropdown which need an extra thing (in its case, its selected option idx).

    private int maxWidthCoverage;

    public ChildLayout(List<(UIElementType type, UIElementDescription desc)> uiElements, int posX, int posY, int layoutWidth, int layoutHeight, Drawable? parent) : base(posX, posY, layoutWidth, layoutHeight, parent)
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
                    layout.Selectable(id, selDesc.text, selDesc.width ?? maxWidthCoverage, selDesc.height ?? 25, selDesc.onSelect, id);
                    break;
                case UIElementType.Toggle:
                    ToggleDesc togDesc = (ToggleDesc)desc;
                    layout.Toggle(id, togDesc.startingState, togDesc.text, togDesc.width ?? maxWidthCoverage, togDesc.height ?? 20, togDesc.onToggle, id);
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
            }
        }

        for (int i = 0; i < uiElements.Count; i++)
        {
            if (uiElements[i].elemType == UIElementType.Group)
            {
                HorizontalGroupDesc gDesc = (HorizontalGroupDesc)uiElements[i].elemDesc;
                maxWidthCoverage = (int)((float)layoutWidth / gDesc.uiElements.Count);

                layout.BeginHorizontal(gDesc.spacing);
                {
                    for (int j = 0; j < gDesc.uiElements.Count; j++)
                    {
                        maxWidthCoverage = (int)((float)(layoutWidth - layout.CurrentWidth()) / (gDesc.uiElements.Count - j));

                        (int id, object? payload) = ids[i][j];
                        DrawAccType(id, gDesc.uiElements[j].elemType, gDesc.uiElements[j].elemDesc, i, j);
                        ids[i][j] = (id, payload);
                    }
                }
                layout.EndHorizontal(25);
            }
            else
            {
                maxWidthCoverage = layoutWidth;
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
}
