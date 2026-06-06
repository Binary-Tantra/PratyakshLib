using System.Numerics;
using Raylib_cs;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary;

public abstract class EditorObject : Drawable, IInteractable
{
    private readonly int id;
    protected bool treeInteractable = true;
    protected bool selfInteractable = false;

    public int Id { get => id; }

    public Action OnDeleteObject;

    public EditorObject(Drawable? parent) : base(parent)
    {
        id = IdGen.GetNewID();
    }

    public virtual Drawable? HitTest(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        if (!treeInteractable)
            return null;

        Drawable? result = OnChildrenHitTest(mouseScreenPosition, mouseWorldPosition);

        if (result != null)
            return result;

        if (!selfInteractable)
            return null;

        Vector2 mousePos = InteractionUseWorldPos() ? mouseWorldPosition : mouseScreenPosition;
        if (Raylib.CheckCollisionPointRec(mousePos, GetInteractableRect()))
            result = this;

        return result;
    }

    protected virtual Drawable? OnChildrenHitTest(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition) { return null; }

    protected virtual Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, 0, 0);
    }

    public void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate() { }

    public override void Delete()
    {
        OnDelete();
        OnDeleteObject?.Invoke();
    }

    protected virtual void OnDelete() { }

    protected abstract bool InteractionUseWorldPos();

    public Rectangle GetInteractableRect()
    {
        return OnGetInteractionRect();
    }
}
