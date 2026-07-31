using Raylib_cs;
using System.Numerics;

namespace RaylibNodeLibrary.UI;

public interface IContextable
{
    public string GetContextMenu();
}

public class ContextMenu : UIBase
{
    private List<(string name, object payload)> menuItems;
    private List<Button> menuButtons;

    private static int buttonWidth = 200;
    private static int buttonHeight = 25;

    public ContextMenu(int posX, int posY, List<(string name, object payload)> menuItems, Action<Button> onButtonPressed, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, buttonWidth, buttonHeight * menuItems.Count, parent, parentBasis)
    {
        this.menuItems = menuItems;
        menuButtons = [];

        for (int i = 0; i < menuItems.Count; i++)
        {

            menuButtons.Add(new Button(0, i * buttonHeight, buttonWidth, buttonHeight, menuItems[i].name, (button) => onButtonPressed?.Invoke(button), menuItems[i].payload, parent: this));
        }
    }

    protected override void OnDraw()
    {
        for (int i = 0; i < menuButtons.Count; i++)
            menuButtons[i].Render();
    }

    protected override Drawable? OnChildrenHitTest(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        for (int i = menuButtons.Count - 1; i >= 0; i--)
        {
            var hit = menuButtons[i].HitTest(mouseScreenPosition, mouseWorldPosition);
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
