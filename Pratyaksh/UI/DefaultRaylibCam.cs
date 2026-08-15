using System.Numerics;
using Pratyaksh.Core;
using Raylib_cs;

namespace Pratyaksh.UI;

public class DefaultRaylibCam : EditorObject, IWorldToScreenTransformer
{
    private Camera2D rCam2D;

    private float screenWidth;
    private float screenHeight;

    public Camera2D RaylibCam2D { get => rCam2D; }

    public override Core.Rectangle InteractionRect => new(0, 0, screenWidth, screenHeight);

    public DefaultRaylibCam(float screenWidth, float screenHeight, Drawable? parent = null) : base(parent)
    {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;

        selfInteractable = true; // For camera drag
    }

    public void Setup()
    {
        rCam2D = new Camera2D
        {
            Target = Vector2.Zero,
            Offset = Vector2.Zero,
            Rotation = 0.0f,
            Zoom = 1.0f
        };
    }

    public override bool InteractionUseWorldPos()
    {
        return false;
    }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        throw new NotImplementedException();
    }

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        throw new NotImplementedException();
    }

    public Core.Rectangle WorldToScreen(Core.Rectangle worldRect)
    {
        Vector2 screenPos = new(worldRect.X, worldRect.Y);
        Vector2 size = new(worldRect.Width, worldRect.Height);

        Vector2 otherCornerPos = screenPos + size;

        Vector2 worldPos = Raylib.GetWorldToScreen2D(screenPos, rCam2D);
        Vector2 worldOtherCorner = Raylib.GetWorldToScreen2D(otherCornerPos, rCam2D);

        return new Core.Rectangle(worldPos.X, worldPos.Y, worldOtherCorner.X - worldPos.X, worldOtherCorner.Y - worldPos.Y);
    }

    public Core.Rectangle ScreenToWorld(Core.Rectangle screenRect)
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