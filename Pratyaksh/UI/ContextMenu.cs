using System.Numerics;
using Pratyaksh.Core;
using Pratyaksh.UI.UIElements;

namespace Pratyaksh.UI;

public class ContextMenu : UILayoutBase
{
    private List<(string name, object payload)> menuItems;
    private List<Button> menuButtons;

    private static int buttonWidth = 200;
    private static int buttonHeight = 25;

    protected override string PanelName => throw new NotImplementedException();

    public ContextMenu(int posX, int posY, List<(string name, object payload)> menuItems, Action<Button> onButtonPressed, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, buttonWidth, buttonHeight * menuItems.Count, parent, parentBasis)
    {
        this.menuItems = menuItems;
        menuButtons = [];

        for (int i = 0; i < menuItems.Count; i++)
        {

            menuButtons.Add(new Button(0, i * buttonHeight, buttonWidth, buttonHeight, menuItems[i].name, (button) => onButtonPressed?.Invoke(button), menuItems[i].payload, parent: this));
        }
    }

    public override void OnDrawLayout()
    {
        for (int i = 0; i < menuButtons.Count; i++)
            menuButtons[i].Render();
    }

    protected override Drawable? OnChildrenHitTest(IWorldToScreenTransformer transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        for (int i = menuButtons.Count - 1; i >= 0; i--)
        {
            var hit = menuButtons[i].HitTest(transformer, mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        return null;
    }

    protected override void OnDelete()
    {
        for (int i = 0; i < menuButtons.Count; i++)
            menuButtons[i].Delete();
    }
}
