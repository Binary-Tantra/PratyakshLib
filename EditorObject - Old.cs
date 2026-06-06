/*using Raylib_cs;
using System.Numerics;

namespace RaylibNodeLibrary;

public abstract class EditorObject : Drawable, IInteractable
{
    protected bool interactable = true;
    protected int interactablePriority = 0;
    
    protected bool interactableUpdate = false;
    protected bool interactableBlockOthers = true;

    private List<InputAction> actions;

    private bool wasRegisteredAtLeastOnce = false;

    public EditorObject(Drawable? parent) : base(parent)
    {
        actions = [];
    }

    public virtual Drawable? HitTest(Vector2 mousePosition)
    {
        if (!interactable)
            return null;

        Drawable? result = null;
        if (Raylib.CheckCollisionPointRec(mousePosition, GetInteractableRect()))
            result = this;

        return OnChildrenHitTest(mousePosition) ?? result;
    }

    protected virtual Drawable? OnChildrenHitTest(Vector2 mousePosition) { return null; }

    protected virtual Rectangle GetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, 10, 10);
    }

    protected void AddInteractionAction(InputAction action)
    {
        if (interactable)
        {
            actions.Add(action);
            InteractionManager.RegisterInteractable(this, action);
            wasRegisteredAtLeastOnce = true;
        }
    }

    public void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate() { }

    public override void Delete()
    {
        if (wasRegisteredAtLeastOnce)
            InteractionManager.UnregisterInteractable(this);
        
        OnDelete();
    }

    protected virtual void OnDelete() { }

    public int GetInteractablePriority()
    {
        return interactablePriority;
    }

    public bool UseWorldSpacePositions()
    {
        return ShouldUseWorldSpacePositions();
    }

    protected abstract bool ShouldUseWorldSpacePositions();

    public Rectangle GetInteractableRect()
    {
        return GetInteractionRect();
    }

    public bool IsInteractionUpdatable()
    {
        return interactableUpdate;
    }

    public bool DoesInteractionBlockOthers()
    {
        return interactableBlockOthers;
    }

    public void OnInteractionStart(InputContext inputContext, InputAction action)
    {
        if (action is MouseInputAction mouseAction)
            OnMouseDown(mouseAction.button, inputContext);

        if (action is KeyboardInputAction kbAction)
            OnKeyDown(kbAction.key, inputContext);
    }

    public void OnInteractionUpdate(InputContext inputContext, InputAction action)
    {
        if (action is MouseInputAction mouseAction)
            OnMouseRemain(mouseAction.button, inputContext);

        if (action is KeyboardInputAction kbAction)
            OnKeyRemain(kbAction.key, inputContext);
    }

    public void OnInteractionEnd(InputContext inputContext, InputAction action)
    {
        if (action is MouseInputAction mouseAction)
            OnMouseUp(mouseAction.button, inputContext);

        if (action is KeyboardInputAction kbAction)
            OnKeyUp(kbAction.key, inputContext);
    }

    protected virtual void OnMouseDown(MouseButton mouseButton, InputContext inputContext) { }
    protected virtual void OnMouseRemain(MouseButton mouseButton, InputContext inputContext) { }
    protected virtual void OnMouseUp(MouseButton mouseButton, InputContext inputContext) { }

    protected virtual void OnKeyDown(KeyboardKey key, InputContext inputContext) { }
    protected virtual void OnKeyRemain(KeyboardKey key, InputContext inputContext) { }
    protected virtual void OnKeyUp(KeyboardKey key, InputContext inputContext) { }

    public List<InputAction> GetInputActions()
    {
        return actions;
    }
}
*/