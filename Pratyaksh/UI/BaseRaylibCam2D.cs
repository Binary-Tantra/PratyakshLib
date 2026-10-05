using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI;

public abstract class BaseRaylibCam2D : EditorObject, IWorldToScreenTransformer2D
{
    protected float screenWidth;
    protected float screenHeight;

    protected Raylib_cs.Camera2D rCam2D;

    public Raylib_cs.Camera2D RaylibCam2D { get => rCam2D; }

    public override Rectangle InteractionRect => new(0, 0, screenWidth, screenHeight);

    public BaseRaylibCam2D(float screenWidth, float screenHeight, Transform? parent = null) : base(parent)
    {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;

        rCam2D = new Raylib_cs.Camera2D
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
        return Raylib_cs.Raylib.GetScreenToWorld2D(screenPos, rCam2D);
    }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        return Raylib_cs.Raylib.GetWorldToScreen2D(worldPos, rCam2D);
    }

    public Rectangle ScreenToWorld(Rectangle screenRect)
    {
        Vector2 screenPos = new(screenRect.X, screenRect.Y);
        Vector2 screenSize = new(screenRect.Width, screenRect.Height);

        Vector2 otherCornerScreenPos = screenPos + screenSize;

        Vector2 worldPos = Raylib_cs.Raylib.GetScreenToWorld2D(screenPos, rCam2D);
        Vector2 otherCornerWorldPos = Raylib_cs.Raylib.GetScreenToWorld2D(otherCornerScreenPos, rCam2D);

        return new Rectangle(worldPos.X, worldPos.Y, otherCornerWorldPos.X - worldPos.X, otherCornerWorldPos.Y - worldPos.Y);
    }

    public Rectangle WorldToScreen(Rectangle worldRect)
    {
        Vector2 worldPos = new(worldRect.X, worldRect.Y);
        Vector2 worldSize = new(worldRect.Width, worldRect.Height);

        Vector2 otherCornerWorldPos = worldPos + worldSize;

        Vector2 screenPos = Raylib_cs.Raylib.GetWorldToScreen2D(worldPos, rCam2D);
        Vector2 otherCornerScreenPos = Raylib_cs.Raylib.GetWorldToScreen2D(otherCornerWorldPos, rCam2D);

        return new Rectangle(screenPos.X, screenPos.Y, otherCornerScreenPos.X - screenPos.X, otherCornerScreenPos.Y - screenPos.Y);
    }
}
