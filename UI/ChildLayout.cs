using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class ChildLayout : UILayoutBase
{
    private List<(UIElementType elemType, UIElementDescription elemDesc)> uiElements;
    private readonly List<List<int>> ids;

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
                List<int> gdIds = [];

                for (int j = 0; j < gd.uiElements.Count; j++)
                    gdIds.Add(IdGen.GetNewID());

                ids.Add(gdIds);
            }
            else ids.Add([IdGen.GetNewID()]);
        }
    }

    public override void OnDrawLayout()
    {
        void DrawAccType(int id, UIElementType type, UIElementDescription desc)
        {
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
                    layout.InputField(id, ifDesc.placeholderText, ifDesc.text, ifDesc.width ?? maxWidthCoverage, ifDesc.height ?? 25);
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
                    layout.Dropdown(id, ddDesc.options, ddDesc.selectedIndex, ddDesc.width ?? maxWidthCoverage, ddDesc.height ?? 25, ddDesc.onSelectionChanged, id);
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
                        DrawAccType(ids[i][j], gDesc.uiElements[j].elemType, gDesc.uiElements[j].elemDesc);
                    }
                }
                layout.EndHorizontal(25);
            }
            else
            {
                maxWidthCoverage = layoutWidth;
                DrawAccType(ids[i][0], uiElements[i].elemType, uiElements[i].elemDesc);
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
