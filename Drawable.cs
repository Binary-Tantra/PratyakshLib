using System.Numerics;

namespace RaylibNodeLibrary;

public abstract class Drawable
{
    private Drawable? parent;
    private bool shouldDraw = true;
    
    protected Vector2 relativePosition;

    public bool ShouldDraw { get => shouldDraw; }
    public Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }
    public Vector2 Position { get => (parent?.Position ?? Vector2.Zero) + relativePosition; }
    public Drawable? Parent { get => parent; }

    public Drawable(Drawable? parent = null)
    {
        this.parent = parent;
    }

    public void Show()
    {
        shouldDraw = true;
    }

    public void Hide()
    {
        shouldDraw = false;
    }

    public void Toggle()
    {
        shouldDraw = !shouldDraw;
    }

    public void Render()
    {
        //Raylib_cs.Raylib.DrawRectangle((int)Position.X - 5, (int)Position.Y - 5, 10, 10, Raylib_cs.Color.Red);
        if (shouldDraw) OnDraw();
    }

    protected virtual void OnDraw() { }

    public abstract void Delete();
}
