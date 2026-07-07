using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public enum UIElementType
{
    Text, Button, InputField, Selectable, Toggle, Dropdown, Group
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

public class ToggleDesc : RectUIEDescription
{
    public bool startingState;
    public Action<Toggle> onToggle;

    public ToggleDesc(string text, bool startingState, int? width, int? height, Action<Toggle> onToggle) : base(text, width, height)
    {
        this.startingState = startingState;
        this.onToggle = onToggle;
    }
}

public class DropdownDesc : RectUIEDescription
{
    public string[] options;
    public int selectedIndex;
    public Action<Dropdown, int> onSelectionChanged;

    public DropdownDesc(string[] options, int selectedIndex, int? width, int? height, Action<Dropdown, int> onSelectionChanged) : base("", width, height)
    {
        this.options = options;
        this.selectedIndex = selectedIndex;
        this.onSelectionChanged = onSelectionChanged;
    }
}

public class HorizontalGroupDesc : RectUIEDescription
{
    public int spacing;
    public List<(UIElementType elemType, UIElementDescription elemDesc)> uiElements;

    public HorizontalGroupDesc(string layoutName, int spacing, List<(UIElementType, UIElementDescription)> uiElements, int? width, int? height) : base(layoutName, width, height)
    {
        this.spacing = spacing;
        this.uiElements = uiElements;
    }
}

public abstract class UIBase : EditorObject, IPointerVisitable
{
    protected bool hovered;

    protected UIBase(Drawable? parent) : base(parent)
    {
        hovered = false;
    }

    public override bool InteractionUseWorldPos()
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
