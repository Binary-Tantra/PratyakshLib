using Pratyaksh.Core;
using System.Numerics;

namespace RaylibNodeLibrary;

public class EditorCamera2D : EditorObject, IPointerInteractable, IDragable, IScrollable, IWorldToScreenTransformer
{
    private Raylib_cs.Camera2D rCam2D;

    private float camZoomSpeed = 0.10f;
    private Vector2 camZoomBounds = new(0.5f, 1.5f);

    private Vector2 xBounds = new(-5000, 5000);
    private Vector2 yBounds = new(-5000, 5000);

    private bool panning;

    private float screenWidth;
    private float screenHeight;

    public Raylib_cs.Camera2D RaylibCam2D { get => rCam2D; }

    public override Rectangle InteractionRect => new(0, 0, screenWidth, screenHeight);

    public EditorCamera2D(float screenWidth, float screenHeight, Drawable? parent = null) : base(parent)
    {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;

        selfInteractable = true; // For camera drag
    }

    public void Setup()
    {
        rCam2D = new Raylib_cs.Camera2D
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

            Vector2 worldAfter = Raylib_cs.Raylib.GetScreenToWorld2D(mouseScreenPos, rCam2D);

            SetCamTarget(rCam2D.Target + worldBefore - worldAfter);
        }
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
        if (evt.MouseButton != MouseButton.Right)
            return false;

        Engine.Instance.InteractionManager.CapturePointer(this);
        panning = true;

        return true;
    }

    public void OnDrag(PointerInteractEventData evt)
    {
        if (panning) UpdatePan(evt.ScreenDelta);
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Right)
            return false;

        panning = false;
        Engine.Instance.InteractionManager.ReleasePointer();

        return true;
    }

    public bool OnScroll(ScrollEventData evt)
    {
        UpdateZoom(evt.ScreenPosition, evt.WorldPosition, evt.MouseWheel.Y);
        return true;
    }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        throw new NotImplementedException();
    }

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        throw new NotImplementedException();
    }

    public Rectangle WorldToScreen(Rectangle worldRect)
    {
        Vector2 screenPos = new(worldRect.X, worldRect.Y);
        Vector2 size = new(worldRect.Width, worldRect.Height);

        Vector2 otherCornerPos = screenPos + size;

        Vector2 worldPos = Raylib_cs.Raylib.GetWorldToScreen2D(screenPos, rCam2D);
        Vector2 worldOtherCorner = Raylib_cs.Raylib.GetWorldToScreen2D(otherCornerPos, rCam2D);

        return new Rectangle(worldPos.X, worldPos.Y, worldOtherCorner.X - worldPos.X, worldOtherCorner.Y - worldPos.Y);
    }

    public Rectangle ScreenToWorld(Rectangle screenRect)
    {
        throw new NotImplementedException();
    }

    public float GetWidth()
    {
        return screenWidth;
    }

    public float GetHeight()
    {
        return screenHeight;
    }

    public Vector2 GetScreenSize()
    {
        return new Vector2(screenWidth, screenHeight);
    }

    public void SetScreenSize(Vector2 screenSize)
    {
        screenWidth = screenSize.X;
        screenHeight = screenSize.Y;
    }
}