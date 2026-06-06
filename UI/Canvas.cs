using System.Numerics;
using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class Canvas : UIBase, IPointerInteractable
{
    private VariablePanel variablePanel;
    private InspectorPanel inspectorPanel;
    
    private ContextMenu? contextMenu;
    public Action<Button, Drawable?> onContextBtnPressed;

    public Canvas(Drawable? parent, Action<Button, Drawable?> onContextBtnPressed, Action<int?> onSelectVariable, Action onAddNewVar, Action<int> onRemoveVar, Action<int, string> onRenameVariable) : base(parent)
    {
        this.onContextBtnPressed = onContextBtnPressed;

        variablePanel = new VariablePanel(10, 20, onSelectVariable, onAddNewVar, onRemoveVar, onRenameVariable);
        inspectorPanel = new InspectorPanel(Engine.ScreenWidth - 200 - 10, 20);

        Engine.OnAnyPointerDown += HandleGlobalClickAway;
    }

    protected override void OnUpdate()
    {
        contextMenu?.Update();
        variablePanel.Update();
        inspectorPanel.Update();
    }

    protected override void OnDraw()
    {
        variablePanel.Render();
        inspectorPanel.Render();
        contextMenu?.Render();
    }

    protected override Drawable? OnChildrenHitTest(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        var hit = contextMenu?.HitTest(mouseScreenPosition, mouseWorldPosition) ?? null;
        if (hit != null) return hit;

        hit = inspectorPanel.HitTest(mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = variablePanel.HitTest(mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        return null;
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(0, 0, 0, 0);
    }

    protected override bool InteractionUseWorldPos()
    {
        return false;
    }

    protected override void OnDelete()
    {
        Engine.OnAnyPointerDown -= HandleGlobalClickAway;
    }

    private void HandleGlobalClickAway(PointerInteractEventData evt, EditorObject? target)
    {
        if (contextMenu == null) return;

        bool clickedInsideMenu = false;

        EditorObject? current = target;
        while (current != null && current is UIBase)
        {
            if (current == contextMenu)
            {
                clickedInsideMenu = true;
                break;
            }

            current = current.Parent as EditorObject;
        }

        if (!clickedInsideMenu && evt.mouseButton == MouseButton.Left)
            RemoveCtxMenu();
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        return false;
    }

    public void RemoveCtxMenu()
    {
        if (contextMenu != null)
        {
            contextMenu.Delete();
            Engine.UIElements.Remove(contextMenu);
            contextMenu = null;
        }
    }

    private void OnCtxBtnPressed(Button button, Drawable? drawable)
    {
        onContextBtnPressed?.Invoke(button, drawable);
        RemoveCtxMenu();
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Right)
            return false;

        RemoveCtxMenu();

        if (InteractionManager.CurrentlyHit == null)
        {
            List<(string, object)> mis = Engine.CurrentlySelectedVarId == null ? [("Add Node", 0)] : [("Add Node", 0), ("Get Var", Engine.CurrentlySelectedVarId)];
            ContextMenu cm = new(mis, (button) => OnCtxBtnPressed(button, null), null);
            Engine.UIElements.Add(cm);
            contextMenu = cm;
        }
        else if (InteractionManager.CurrentlyHit is NodeVisual nui)
        {
            ContextMenu cm = new([("Delete", 0)], (button) => OnCtxBtnPressed(button, nui), null);
            Engine.UIElements.Add(cm);
            contextMenu = cm;
        }

        contextMenu?.RelativePosition = evt.ScreenPosition;

        if (contextMenu == null)
            return false;

        return true;
    }
}
