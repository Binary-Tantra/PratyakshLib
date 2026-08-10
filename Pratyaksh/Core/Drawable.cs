using System.Numerics;

namespace Pratyaksh.Core;

public record struct Rectangle(float X, float Y, float Width, float Height)
{
    public readonly bool Contains(Vector2 point)
    {
        return point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
    }
}

public abstract class Drawable
{
    protected Drawable? parent;
    private bool shouldDraw = true;
    
    private Vector2 relativePosition;

    public bool ShouldDraw { get => shouldDraw; }
    
    public virtual Vector2 RelativePosition { get => relativePosition; set => relativePosition = value; }

    public virtual Vector2 Position
    {
        get
        {
            Vector2 parentPos = parent?.Position ?? Vector2.Zero;
            return parentPos + relativePosition;
        }
        set
        {
            Vector2 required = value;
            Vector2 diff = required - parent?.Position ?? Vector2.Zero;
            relativePosition = diff;
        }
    }
    
    public Drawable? Parent
    {
        get => parent;
        set
        {
            if (value == parent)
                return;

            OnSetParent(value, parent, true);
        }
    }

    public Drawable(Drawable? parent = null)
    {
        this.parent = parent;
    }

    public void SetParent(Drawable? newParent, bool preservePosition)
    {
        if (newParent == parent)
            return;

        OnSetParent(newParent, parent, preservePosition);
    }

    protected virtual void OnSetParent(Drawable? newParent, Drawable? oldParent, bool preservePosition)
    {
        Vector2 currAbsPos = Position;
        parent = newParent;

        if (preservePosition) Position = currAbsPos;    // Recalculate relative position based on new parent
        else relativePosition = Vector2.Zero;           // Reset relative position if not preserving
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
        //DrawRectangle((int)Position.X - 5, (int)Position.Y - 5, 10, 10, Color.Red);
        if (shouldDraw) OnDraw();
    }

    protected virtual void OnDraw() { }

    public abstract void Delete();
}
