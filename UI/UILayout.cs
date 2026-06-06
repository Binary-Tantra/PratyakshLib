using System.Numerics;
using Raylib_cs;
using RlSimpleLayoutEngine;

namespace RaylibNodeLibrary.UI;

public abstract class UILayout : UIBase, IPointerInteractable, IDragable
{
    protected RlSimpleLayout layout;

    protected int layoutWidth;
    protected int layoutHeight;

    private bool isDragging = false;
    private Vector2 dragOffset;

    public int LayoutWidth { get => layoutWidth; }
    public int LayoutHeight { get => layoutHeight; }

    public UILayout(int posX, int posY, int layoutWidth, int layoutHeight, Drawable? parent) : base(parent)
    {
        selfInteractable = true;
        RelativePosition = new Vector2(posX, posY);

        this.layoutWidth = layoutWidth;
        this.layoutHeight = layoutHeight;

        layout = new RlSimpleLayout();
        layout.InitFont(Raylib.GetFontDefault());
    }

    protected override void OnDraw()
    {
        Raylib.BeginScissorMode((int)Position.X, (int)Position.Y, layoutWidth, layoutHeight);
        {
            layout.BeginHorizontalEx(0, (int)Position.X);
            {
                layout.BeginVerticalEx(0, (int)Position.Y);
                {
                    OnDrawLayout();
                }
                layout.EndVertical(layoutWidth);
            }
            layout.EndHorizontal(layoutHeight);
        }
        Raylib.EndScissorMode();
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

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, layoutWidth, layoutHeight);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
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
        {
            relativePosition.X = evt.ScreenPosition.X - dragOffset.X;
            relativePosition.Y = evt.ScreenPosition.Y - dragOffset.Y;
        }
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
