using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public enum UIElementType
{
    Text, Button, InputField, Selectable
}

public abstract class UIElementDescription
{
    public string text;

    protected UIElementDescription(string text)
    {
        this.text = text;
    }
}

public class TextDesc : UIElementDescription
{
    public Color color;

    public TextDesc(string text, Color color) : base(text)
    {
        this.color = color;
    }
}

public class RectUIEDescription : UIElementDescription
{
    public int? width;
    public int? height;

    public RectUIEDescription(string text, int? width, int? height) : base(text)
    {
        this.width = width;
        this.height = height;
    }
}

public class ButtonDesc : RectUIEDescription
{
    public Action<Button> onClick;

    public ButtonDesc(string text, int? width, int? height, Action<Button> onClick) : base(text, width, height)
    {
        this.onClick = onClick;
    }
}

public class InputFieldDesc : RectUIEDescription
{
    public string placeholderText;

    public InputFieldDesc(string placeholderText, string inputFieldText, int? width, int? height) : base(inputFieldText, width, height)
    {
        this.placeholderText = placeholderText;
    }
}

public class SelectableDesc : RectUIEDescription
{
    public Action<Selectable> onSelect;

    public SelectableDesc(string text, int? width, int? height, Action<Selectable> onSelect) : base(text, width, height)
    {
        this.onSelect = onSelect;
    }
}

public abstract class UIBase : EditorObject, IPointerVisitable
{
    protected bool hovered;

    protected UIBase(Drawable? parent) : base(parent)
    {
        hovered = false;
    }

    protected override bool InteractionUseWorldPos()
    {
        return false;
    }

    public void OnMouseEnter(PointerVisitEventData evt)
    {
        hovered = true;
        OnMouseEnter();
    }

    public void OnMouseExit(PointerVisitEventData evt)
    {
        hovered = false;
        OnMouseExit();
    }

    protected virtual void OnMouseEnter() { }

    protected virtual void OnMouseExit() { }
}
