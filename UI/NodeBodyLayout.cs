using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class NodeBodyLayout : UILayout
{
    private List<(UIElementType elemType, UIElementDescription elemDesc)> uiElements;
    private readonly List<int> ids;

    public NodeBodyLayout(List<(UIElementType, UIElementDescription)> uiElements, int posX, int posY, int layoutWidth, int layoutHeight, Drawable? parent) : base(posX, posY, layoutWidth, layoutHeight, parent)
    {
        selfInteractable = false;
        this.uiElements = uiElements;

        ids = [];
        for (int i = 0; i < uiElements.Count; i++)
            ids.Add(IdGen.GetNewID());
    }

    public override void OnDrawLayout()
    {
        layout.BeginHorizontal(0);
        {
            layout.BeginVerticalEx(5, (int)Position.Y);
            {
                for (int i = 0; i < uiElements.Count; i++)
                {
                    switch (uiElements[i].elemType)
                    {
                        case UIElementType.Text:
                            TextDesc textDesc = (TextDesc)uiElements[i].elemDesc;
                            layout.Text(textDesc.text, textDesc.color);
                            break;
                        case UIElementType.Button:
                            ButtonDesc buttonDesc = (ButtonDesc)uiElements[i].elemDesc;
                            layout.Button(ids[i], buttonDesc.text, buttonDesc.width ?? layoutWidth, buttonDesc.height ?? 25, buttonDesc.onClick, ids[i]);
                            break;
                        case UIElementType.InputField:
                            InputFieldDesc ifDesc = (InputFieldDesc)uiElements[i].elemDesc;
                            layout.InputField(ids[i], ifDesc.placeholderText, ifDesc.text, ifDesc.width ?? layoutWidth, ifDesc.height ?? 25);
                            break;
                        case UIElementType.Selectable:
                            SelectableDesc selDesc = (SelectableDesc)uiElements[i].elemDesc;
                            layout.Selectable(ids[i], selDesc.text, selDesc.width ?? layoutWidth, selDesc.height ?? 25, selDesc.onSelect, ids[i]);
                            break;
                    }
                }
            }
            layout.EndVertical(layoutWidth);
        }
        layout.EndHorizontal(layoutHeight);
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
