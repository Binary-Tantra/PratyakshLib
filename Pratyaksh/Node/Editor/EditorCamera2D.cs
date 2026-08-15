using System.Numerics;
using Pratyaksh.Core;
using Pratyaksh.UI;

namespace Pratyaksh.Node.Editor;

public class EditorCamera2D : BaseRaylibCam, IPointerInteractable, IDragable, IScrollable
{
    private float camZoomSpeed = 0.10f;
    private Vector2 camZoomBounds = new(0.5f, 1.5f);

    private Vector2 xBounds = new(-5000, 5000);
    private Vector2 yBounds = new(-5000, 5000);

    private bool panning;

    public EditorCamera2D(float screenWidth, float screenHeight, Drawable? parent = null) : base(screenWidth, screenHeight, parent)
    {
        selfInteractable = true; // For camera drag
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
}