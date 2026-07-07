namespace RaylibNodeLibrary;

using System.Numerics;
using Raylib_cs;

public class EditorCamera2D : EditorObject, IPointerInteractable, IDragable, IScrollable
{
    private Camera2D rCam2D;

    private float camZoomSpeed = 0.10f;
    private Vector2 camZoomBounds = new(0.5f, 1.5f);

    private Vector2 xBounds = new(-5000, 5000);
    private Vector2 yBounds = new(-5000, 5000);

    private bool panning;

    public Camera2D RaylibCam2D { get => rCam2D; }

    public EditorCamera2D(int screenWidth, int screenHeight, Drawable? parent = null) : base(parent)
    {
        rCam2D = new Camera2D
        {
            Target = Vector2.Zero,
            Offset = Vector2.Zero,
            Rotation = 0.0f,
            Zoom = 1.0f
        };
    }

    private void SetCamTarget(Vector2 newCT)
    {
        rCam2D.Target = newCT;

        rCam2D.Target.X = Math.Clamp(rCam2D.Target.X, xBounds.X, xBounds.Y);
        rCam2D.Target.Y = Math.Clamp(rCam2D.Target.Y, yBounds.X, yBounds.Y);
    }

    private void UpdatePan(Vector2 mouseScreenDelta)
    {
        SetCamTarget(rCam2D.Target - mouseScreenDelta / rCam2D.Zoom);
    }

    private void UpdateZoom(Vector2 mouseScreenPos, Vector2 mouseWorldPos, float mouseWheel)
    {
        if (mouseWheel != 0)
        {
            Vector2 worldBefore = mouseWorldPos;

            rCam2D.Zoom += mouseWheel * camZoomSpeed * rCam2D.Zoom;
            rCam2D.Zoom = Math.Clamp(rCam2D.Zoom, camZoomBounds.X, camZoomBounds.Y);

            Vector2 worldAfter = Raylib.GetScreenToWorld2D(mouseScreenPos, rCam2D);

            SetCamTarget(rCam2D.Target + worldBefore - worldAfter);
        }
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(0, 0, 0, 0);
    }

    public override bool InteractionUseWorldPos()
    {
        return false;
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        return false;
    }

    public bool OnDragStart(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Right)
            return false;

        InteractionManager.CapturePointer(this);
        panning = true;

        return true;
    }

    public void OnDrag(PointerInteractEventData evt)
    {
        if (panning) UpdatePan(evt.ScreenDelta);
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Right)
            return false;

        panning = false;
        InteractionManager.ReleasePointer();

        return true;
    }

    public bool OnScroll(ScrollEventData evt)
    {
        UpdateZoom(evt.ScreenPosition, evt.WorldPosition, evt.MouseWheel.Y);
        return true;
    }

    public Rectangle GetWorldToScreenRect(Rectangle target)
    {
        return GetWorldToScreenRect(new Vector2(target.X, target.Y), new Vector2(target.Width, target.Height));
    }

    public Rectangle GetWorldToScreenRect(Vector2 screenPos, Vector2 size)
    {
        Vector2 otherCornerPos = screenPos + size;

        Vector2 worldPos = Raylib.GetWorldToScreen2D(screenPos, rCam2D);
        Vector2 worldOtherCorner = Raylib.GetWorldToScreen2D(otherCornerPos, rCam2D);

        return new Rectangle(worldPos.X, worldPos.Y, worldOtherCorner.X - worldPos.X, worldOtherCorner.Y - worldPos.Y);
    }
}
