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

        Drawable? ancestor = Parent;
        while (ancestor != null)
        {
            if (ancestor is IClippable clippable)
            {
                if (!Raylib.CheckCollisionPointRec(mousePos, clippable.GetScissorRect()))
                    return null;
            }
            ancestor = ancestor.Parent;
        }

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

    public abstract bool InteractionUseWorldPos();

    protected bool CheckAncestorsForInteractWorldPos()
    {
        bool worldSpace = false;
        Drawable? par = Parent;

        // Ooof...TODO: This should not be needed...but currently it works.
        while (par != null)
        {
            if (par is EditorObject ob && ob.InteractionUseWorldPos())
            {
                worldSpace = true;
                break;
            }
            else par = par.Parent;
        }

        return worldSpace;
    }

    public Rectangle GetInteractableRect()
    {
        Rectangle finalRect = OnGetInteractionRect();

        if (!InteractionUseWorldPos()) // Only check if self doesn't use world space. If self does use world space...dont check in that case, as that would be wrong.
        {
            bool worldSpace = CheckAncestorsForInteractWorldPos(); // Just check ancestors if they are in world space...don't need to check self in world space, as that happens in the HitTest function already.

            if (worldSpace)
                finalRect = Engine.Camera.GetWorldToScreenRect(finalRect);
        }

        return finalRect;
    }

    public bool IsAncestor(Drawable targetAncestor)
    {
        Drawable? curr = this;
        while (curr != null)
        {
            if (curr == targetAncestor) return true;
            curr = curr.Parent;
        }
        return false;
    }

    public static bool IsAncestor(Drawable? obj, Drawable targetAncestor)
    {
        return obj is EditorObject eo && eo.IsAncestor(targetAncestor);
    }

    public bool IsAncestorType<T>() where T : Drawable
    {
        Drawable? curr = this;
        while (curr != null)
        {
            if (curr is T) return true;
            curr = curr.Parent;
        }
        return false;
    }

    public static bool IsAncestorType<T>(Drawable? obj) where T : Drawable
    {
        return obj is EditorObject eo && eo.IsAncestorType<T>();
    }
}
