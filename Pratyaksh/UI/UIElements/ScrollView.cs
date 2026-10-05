using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

public class ScrollView : UIBase, IScrollable, IPointerInteractable, IDragable, IClippable
{
    private Vector2 scrollOffset;
    private Vector2 contentSize;

    private int scrollbarWidth = 10;
    private bool isScrollBarDragging = false;

    private float dragStartMouseY;
    private float dragStartScrollY;

    public Vector2 ScrollOffset { get => scrollOffset; }

    public ScrollView(int viewWidth, int viewHeight, Transform? parent = null, ParentBasis? parentBasis = null) : base(0, 0, viewWidth, viewHeight, parentBasis, parent)
    {
        selfInteractable = true;
        scrollOffset = Vector2.Zero;
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
        float maxScroll = Math.Max(0, contentSize.Y - UITransform.Size.Y);
        scrollOffset.Y = Math.Clamp(scrollOffset.Y, -maxScroll, 0);
    }

    public Rectangle GetScissorRect(IWorldToScreenTransformer2D transformer)
    {
        Rectangle rect = new(UITransform.X, UITransform.Y, UITransform.Size.X, UITransform.Size.Y);
        bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();

        if (worldSpace)
            rect = transformer.WorldToScreen(rect);

        return rect;
    }

    protected override void OnDraw()
    {
        if (contentSize.Y > UITransform.Size.Y)
        {
            Raylib_cs.Rectangle track = new(UITransform.X + UITransform.Size.X - scrollbarWidth, UITransform.Y, scrollbarWidth, UITransform.Size.Y);
            Raylib_cs.Raylib.DrawRectangleRec(track, new Raylib_cs.Color(40, 40, 40, 255));

            float visibleRatio = UITransform.Size.Y / contentSize.Y;
            float thumbHeight = Math.Max(20, UITransform.Size.Y * visibleRatio);
            float maxScroll = contentSize.Y - UITransform.Size.Y;
            float scrollRatio = maxScroll > 0 ? (-scrollOffset.Y / maxScroll) : 0;
            float thumbY = UITransform.Y + (UITransform.Size.Y - thumbHeight) * scrollRatio;

            Raylib_cs.Rectangle thumb = new(UITransform.X + UITransform.Size.X - scrollbarWidth + 2, thumbY + 2, scrollbarWidth - 4, thumbHeight - 4);
            bool isHoveringTrack = Raylib_cs.Raylib.CheckCollisionPointRec(Engine.Instance.InteractionManager.InputContext.mouseScreenPosition, track);
            Raylib_cs.Color thumbColor = isScrollBarDragging ? new Raylib_cs.Color(120, 120, 120, 255) : ((hovered && isHoveringTrack) ? new Raylib_cs.Color(100, 100, 100, 255) : new Raylib_cs.Color(80, 80, 80, 255));

            Raylib_cs.Raylib.DrawRectangleRounded(thumb, 0.5f, 4, thumbColor);
        }
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left) return false;

        if (contentSize.Y > UITransform.Size.Y)
        {
            Rectangle track = new(UITransform.X + UITransform.Size.X - scrollbarWidth, UITransform.Y, scrollbarWidth, UITransform.Size.Y);
            bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();
            Vector2 hitPos = worldSpace ? evt.WorldPosition : evt.ScreenPosition;

            if (track.Contains(hitPos))
            {
                isScrollBarDragging = true;
                dragStartMouseY = hitPos.Y;
                dragStartScrollY = scrollOffset.Y;
                Engine.Instance.InteractionManager.CapturePointer(this);

                return true;
            }
        }

        return false;
    }

    public bool OnDragStart(PointerInteractEventData evt)
    {
        if (contentSize.Y > UITransform.Size.Y)
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

            float maxScroll = contentSize.Y - UITransform.Size.Y;
            float thumbMoveRange = UITransform.Size.Y - Math.Max(20, UITransform.Size.Y * (UITransform.Size.Y / contentSize.Y));

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
        if (isScrollBarDragging && evt.MouseButton == MouseButton.Left)
        {
            isScrollBarDragging = false;
            Engine.Instance.InteractionManager.ReleasePointer();

            return true;
        }

        return false;
    }
}