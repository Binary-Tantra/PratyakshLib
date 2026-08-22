using System.Numerics;

namespace Pratyaksh.Core;

public abstract class EditorObject : Drawable, IInteractable
{
    private readonly int id;
    protected bool treeInteractable = true;
    protected bool selfInteractable = false;

    public int Id { get => id; }

    public Action OnDeleteObject;

    public virtual Rectangle InteractionRect
    {
        get => new(Position.X, Position.Y, 0, 0);
    }

    public EditorObject(Drawable? parent) : base(parent)
    {
        id = IdGen.GetNewID();
        OnDeleteObject = () => { };
    }

    public virtual Drawable? HitTest(IWorldToScreenTransformer transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        if (!treeInteractable)
            return null;

        Drawable? result = OnChildrenHitTest(transformer, mouseScreenPosition, mouseWorldPosition);

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
                if (!clippable.GetScissorRect(transformer).Contains(mousePos))
                    return null;
            }
            ancestor = ancestor.Parent;
        }

        if (GetInteractableRect(transformer).Contains(mousePos))
            result = this;

        return result;
    }

    protected virtual Drawable? OnChildrenHitTest(IWorldToScreenTransformer transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition) { return null; }

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

    public bool IsSelfInteractable()
    {
        return selfInteractable;
    }

    public Rectangle GetInteractableRect(IWorldToScreenTransformer transformer)
    {
        Rectangle finalRect = InteractionRect;

        if (!InteractionUseWorldPos()) // Condition: Only if we are screen space, execute if.
        {
            bool worldSpace = CheckAncestorsForInteractWorldPos(); // Checking if we (a screen space obj) are attached to a world space object.

            if (worldSpace)
                finalRect = transformer.WorldToScreen(finalRect); // We are a SS object...but because we are attached to a WS obj, we are actually WS! Our pos and size need to be converted to SS.
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

    public bool HasAncestorOfType<T>() where T : Drawable
    {
        Drawable? curr = this;

        while (curr != null)
        {
            if (curr is T) return true;
            curr = curr.Parent;
        }
        
        return false;
    }

    public static bool HasAncestorOfType<T>(Drawable? obj) where T : Drawable
    {
        return obj is EditorObject eo && eo.HasAncestorOfType<T>();
    }

    public static bool IsAnyChildFocused(EditorObject root)
    {
        EditorObject? cur = Engine.Instance.InteractionManager.CurrentlyFocused;

        while (cur != null)
        {
            if (cur == root) return true;
            cur = cur.Parent as EditorObject;
        }

        return false;
    }
}
