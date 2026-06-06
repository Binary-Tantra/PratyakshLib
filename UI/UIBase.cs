namespace RaylibNodeLibrary.UI;

public enum UIElementType
{
    Button, InputField, Selectable
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
