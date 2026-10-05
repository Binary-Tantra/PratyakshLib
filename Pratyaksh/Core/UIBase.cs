using System.Numerics;

namespace Pratyaksh.Core;

public enum ParentBasis
{
    TopLeft, TopCenter, TopRight, Left, Center, Right, BottomLeft, BottomCenter, BottomRight
}

public record struct Rectangle(float X, float Y, float Width, float Height)
{
    public readonly bool Contains(Vector2 point)
    {
        return point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
    }
}

public abstract class UIBase : EditorObject, IPointerVisitable
{
    protected bool hovered;
    private UITransform uiTransform;

    public UITransform UITransform => uiTransform;

    public override Rectangle InteractionRect
    {
        get => new(uiTransform.X, uiTransform.Y, uiTransform.Size.X, uiTransform.Size.Y);
    }

    private static Transform CreateTransform(int relativePosX, int relativePosY, int width, int height, ParentBasis? parentBasis)
    {
        UITransform transform = new(relativePosX, relativePosY, width, height, parentBasis, null);

        return transform;
    }

    protected UIBase(int relativePosX, int relativePosY, int width, int height, ParentBasis? parentBasis = null, Transform? parentTransform = null) : base(CreateTransform(relativePosX, relativePosY, width, height, parentBasis), parentTransform)
    {
        uiTransform = (UITransform)Transform;
        uiTransform.SetOwner(this);

        uiTransform.RelativePosition = new Vector3(relativePosX, relativePosY, 0);

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

    public override void Render()
    {
        base.Render();
        OnDraw();
    }

    protected abstract void OnDraw();
}
