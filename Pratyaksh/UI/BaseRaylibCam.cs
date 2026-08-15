using System.Numerics;
using Pratyaksh.Core;
using Raylib_cs;

namespace Pratyaksh.UI;

public abstract class BaseRaylibCam : EditorObject, IWorldToScreenTransformer
{
    protected float screenWidth;
    protected float screenHeight;

    protected Camera2D rCam2D;

    public Camera2D RaylibCam2D { get => rCam2D; }

    public override Core.Rectangle InteractionRect => new(0, 0, screenWidth, screenHeight);

    public BaseRaylibCam(float screenWidth, float screenHeight, Drawable? parent = null) : base(parent)
    {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;

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

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return Raylib.GetScreenToWorld2D(screenPos, rCam2D);
    }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        return Raylib.GetWorldToScreen2D(worldPos, rCam2D);
    }

    public Core.Rectangle ScreenToWorld(Core.Rectangle screenRect)
    {
        Vector2 screenPos = new(screenRect.X, screenRect.Y);
        Vector2 screenSize = new(screenRect.Width, screenRect.Height);

        Vector2 otherCornerScreenPos = screenPos + screenSize;

        Vector2 worldPos = Raylib.GetScreenToWorld2D(screenPos, rCam2D);
        Vector2 otherCornerWorldPos = Raylib.GetScreenToWorld2D(otherCornerScreenPos, rCam2D);

        return new Core.Rectangle(worldPos.X, worldPos.Y, otherCornerWorldPos.X - worldPos.X, otherCornerWorldPos.Y - worldPos.Y);
    }

    public Core.Rectangle WorldToScreen(Core.Rectangle worldRect)
    {
        Vector2 worldPos = new(worldRect.X, worldRect.Y);
        Vector2 worldSize = new(worldRect.Width, worldRect.Height);

        Vector2 otherCornerWorldPos = worldPos + worldSize;

        Vector2 screenPos = Raylib.GetWorldToScreen2D(worldPos, rCam2D);
        Vector2 otherCornerScreenPos = Raylib.GetWorldToScreen2D(otherCornerWorldPos, rCam2D);

        return new Core.Rectangle(screenPos.X, screenPos.Y, otherCornerScreenPos.X - screenPos.X, otherCornerScreenPos.Y - screenPos.Y);
    }
}
