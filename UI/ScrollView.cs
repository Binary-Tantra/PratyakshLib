using System.Numerics;
using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class ScrollView : UIBase, IScrollable, IPointerInteractable, IDragable
{
    private Vector2 scrollOffset;
    private Vector2 contentSize;
    private Vector2 viewSize;

    private int scrollbarWidth = 10;
    private bool isScrollBarDragging = false;
    private float dragStartMouseY;
    private float dragStartScrollY;

    public Vector2 ViewSize { get => viewSize; }
    public Vector2 ScrollOffset { get => scrollOffset; }

    public ScrollView(int viewWidth, int viewHeight, Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;
        viewSize = new Vector2(viewWidth, viewHeight);
        scrollOffset = Vector2.Zero;
    }

    public void SetViewSize(Vector2 viewSize)
    {
        this.viewSize = viewSize;
    }

    public void SetContentSize(Vector2 contentSize)
    {
        this.contentSize = contentSize;
    }

    public bool OnScroll(ScrollEventData evt)
    {
        float scrollSpeed = 30f;
        scrollOffset.Y += evt.MouseWheel.Y * scrollSpeed;
        ClampScroll();
        return true;
    }

    public void ClampScroll()
    {
        float maxScroll = Math.Max(0, contentSize.Y - viewSize.Y);
        scrollOffset.Y = Math.Clamp(scrollOffset.Y, -maxScroll, 0);
    }

    public Rectangle GetScissorRect()
    {
        Rectangle rect = new Rectangle(Position.X, Position.Y, viewSize.X, viewSize.Y);
        bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();

        if (worldSpace)
            rect = Engine.Camera.GetWorldToScreenRect(rect);

        return rect;
    }

    protected override void OnDraw()
    {
        if (contentSize.Y > viewSize.Y)
        {
            Rectangle track = new Rectangle(Position.X + viewSize.X - scrollbarWidth, Position.Y, scrollbarWidth, viewSize.Y);
            Raylib.DrawRectangleRec(track, new Color(40, 40, 40, 255));

            float visibleRatio = viewSize.Y / contentSize.Y;
            float thumbHeight = Math.Max(20, viewSize.Y * visibleRatio);
            float maxScroll = contentSize.Y - viewSize.Y;
            float scrollRatio = maxScroll > 0 ? (-scrollOffset.Y / maxScroll) : 0;
            float thumbY = Position.Y + (viewSize.Y - thumbHeight) * scrollRatio;

            Rectangle thumb = new Rectangle(Position.X + viewSize.X - scrollbarWidth + 2, thumbY + 2, scrollbarWidth - 4, thumbHeight - 4);
            Color thumbColor = isScrollBarDragging ? new Color(120, 120, 120, 255) : (hovered ? new Color(100, 100, 100, 255) : new Color(80, 80, 80, 255));

            Raylib.DrawRectangleRounded(thumb, 0.5f, 4, thumbColor);
        }
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, viewSize.X, viewSize.Y);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left) return false;

        if (contentSize.Y > viewSize.Y)
        {
            Rectangle track = new Rectangle(Position.X + viewSize.X - scrollbarWidth, Position.Y, scrollbarWidth, viewSize.Y);
            bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();
            Vector2 hitPos = worldSpace ? evt.WorldPosition : evt.ScreenPosition;

            if (Raylib.CheckCollisionPointRec(hitPos, track))
            {
                isScrollBarDragging = true;
                dragStartMouseY = hitPos.Y;
                dragStartScrollY = scrollOffset.Y;
                InteractionManager.CapturePointer(this);

                return true;
            }
        }

        return false;
    }

    public bool OnDragStart(PointerInteractEventData evt)
    {
        if (contentSize.Y > viewSize.Y)
            return isScrollBarDragging;

        return false;
    }

    public void OnDrag(PointerInteractEventData evt)
    {
        if (isScrollBarDragging)
        {
            bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();
            Vector2 hitPos = worldSpace ? evt.WorldPosition : evt.ScreenPosition;
            float deltaY = hitPos.Y - dragStartMouseY;

            float maxScroll = contentSize.Y - viewSize.Y;
            float thumbMoveRange = viewSize.Y - Math.Max(20, viewSize.Y * (viewSize.Y / contentSize.Y));

            if (thumbMoveRange > 0)
            {
                float scrollPerPixel = maxScroll / thumbMoveRange;
                scrollOffset.Y = dragStartScrollY - (deltaY * scrollPerPixel);
                ClampScroll();
            }
        }
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (isScrollBarDragging && evt.mouseButton == MouseButton.Left)
        {
            isScrollBarDragging = false;
            InteractionManager.ReleasePointer();
            return true;
        }

        return false;
    }
}