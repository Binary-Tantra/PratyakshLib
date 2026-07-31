using System.Numerics;
using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public abstract class UILayoutBase : UIBase, IPointerInteractable, IDragable, IClippable
{
    protected LayoutEngine layout;

    private bool isDragging = false;
    private Vector2 dragOffset;

    protected int mainVerticalSpacing = 0;
    protected int horizontalPadding = 0;

    public UILayoutBase(int posX, int posY, int layoutWidth, int layoutHeight, Drawable? parent, ParentBasis? parentBasis = null) : base(posX, posY, layoutWidth, layoutHeight, parent, parentBasis)
    {
        selfInteractable = true;
        layout = new LayoutEngine(this);
    }

    protected override void OnDraw()
    {
        bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();
        Rectangle finalRect = new(Position.X, Position.Y, Width, Height);

        if (worldSpace)
            finalRect = Engine.Camera.GetWorldToScreenRect(finalRect);

        layout.BeginFrame();

        Raylib.BeginScissorMode((int)finalRect.X, (int)finalRect.Y, (int)finalRect.Width, (int)finalRect.Height);
        {
            layout.BeginHorizontalEx(0, (int)Position.X);
            {
                layout.AddSpace(horizontalPadding);

                layout.BeginVerticalEx(mainVerticalSpacing, (int)Position.Y);
                {
                    OnDrawLayout();
                }
                layout.EndVertical(Width);
            }
            layout.EndHorizontal(Height);
        }
        Raylib.EndScissorMode();

        layout.DrawOverlays();
        layout.EndFrame();
    }

    protected override void OnUpdate()
    {
        layout.UpdateLayoutElements();
    }

    protected override void OnDelete()
    {
        layout.ResetLayout();
    }

    public abstract void OnDrawLayout();

    protected override Drawable? OnChildrenHitTest(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        return layout.HitTestElements(mouseScreenPosition, mouseWorldPosition);
    }

    public Rectangle GetScissorRect()
    {
        Rectangle rect = new(Position.X, Position.Y, Width, Height);
        bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();

        if (worldSpace)
            rect = Engine.Camera.GetWorldToScreenRect(rect);

        return rect;
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        return false;
    }

    public bool OnDragStart(PointerInteractEventData evt)
    {
        if (evt.mouseButton == MouseButton.Left)
        {
            isDragging = true;
            dragOffset = new Vector2(evt.ScreenPosition.X - RelativePosition.X, evt.ScreenPosition.Y - RelativePosition.Y);

            InteractionManager.CapturePointer(this);

            return true;
        }

        return false;
    }

    public void OnDrag(PointerInteractEventData evt)
    {
        if (isDragging)
            RelativePosition = new Vector2(evt.ScreenPosition.X - dragOffset.X, evt.ScreenPosition.Y - dragOffset.Y);
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton == MouseButton.Left)
        {
            isDragging = false;
            InteractionManager.ReleasePointer();

            return true;
        }

        return false;
    }
}
