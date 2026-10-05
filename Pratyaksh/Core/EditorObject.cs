using System.Numerics;

namespace Pratyaksh.Core;

public abstract class EditorObject : PratyakshObject, IInteractable
{
    protected bool treeInteractable = true;
    protected bool selfInteractable = false;

    protected Rendering renderingComponent;

    public virtual Rectangle InteractionRect
    {
        get => new(Transform.Position.X, Transform.Position.Y, 0, 0);
    }

    public Rendering RenderingComponent { get => renderingComponent; }

    internal EditorObject(Transform? transform, Transform? parentTransform) : base(transform, parentTransform)
    {
        renderingComponent = AddComponent<Rendering>();
    }

    public EditorObject(Transform? parentTransform) : base(parentTransform)
    {
        renderingComponent = AddComponent<Rendering>();
    }

    public virtual PratyakshObject? HitTest(IWorldToScreenTransformer2D transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        if (!treeInteractable)
            return null;

        PratyakshObject? result = OnChildrenHitTest(transformer, mouseScreenPosition, mouseWorldPosition);

        if (result != null)
            return result;

        if (!selfInteractable)
            return null;

        Vector2 mousePos = InteractionUseWorldPos() ? mouseWorldPosition : mouseScreenPosition;

        Transform? ancestor = Transform.Parent;
        while (ancestor != null)
        {
            if (ancestor.Owner is IClippable clippable)
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

    protected virtual PratyakshObject? OnChildrenHitTest(IWorldToScreenTransformer2D transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition) { return null; }

    public void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate() { }

    public virtual void Render()
    {
        renderingComponent.Render();
    }

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
        Transform? par = Transform.Parent;

        // Ooof...TODO: This should not be needed...but currently it works.
        while (par != null)
        {
            if (par.Owner is EditorObject ob && ob.InteractionUseWorldPos())
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

    public Rectangle GetInteractableRect(IWorldToScreenTransformer2D transformer)
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

    public static bool IsAnyChildFocused(EditorObject root)
    {
        EditorObject? cur = Engine.Instance.InteractionManager.CurrentlyFocused;

        while (cur != null)
        {
            if (cur == root) return true;
            cur = cur.Transform.Parent?.Owner as EditorObject;
        }

        return false;
    }
}
