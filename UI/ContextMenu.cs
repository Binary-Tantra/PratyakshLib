using Raylib_cs;
using System.Numerics;

namespace RaylibNodeLibrary.UI;

public interface IContextable
{
    public string GetContextMenu();
}

public class ContextMenu : UIBase
{
    private List<string> menuItems;
    private List<Button> menuButtons;

    private int buttonWidth = 200;
    private int buttonHeight = 25;

    public ContextMenu(List<string> menuItems, Action<Button> onButtonPressed, Drawable? parent = null) : base(parent)
    {
        this.menuItems = menuItems;
        menuButtons = [];

        for (int i = 0; i < menuItems.Count; i++)
            menuButtons.Add(new Button(buttonWidth, buttonHeight, menuItems[i], (button) => onButtonPressed?.Invoke(button), i, parent: this));
    }

    protected override void OnDraw()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            menuButtons[i].RelativePosition = new Vector2(0, i * buttonHeight);
            menuButtons[i].Render();
        }
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

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, buttonWidth, menuButtons.Count * buttonHeight);
    }

    protected override void OnDelete()
    {
        for (int i = 0; i < menuButtons.Count; i++)
            menuButtons[i].Delete();
    }
}
