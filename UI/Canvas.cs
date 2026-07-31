using System.Numerics;
using Raylib_cs;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class Canvas : UIBase, IPointerInteractable
{
    private VariablePanel variablePanel;
    private InspectorPanel inspectorPanel;
    private DemoPanel demoPanel;
    
    private ContextMenu? contextMenu;
    private SearchMenu? searchMenu;
    public Action<object, EditorObject?> onContextBtnPressed;

    public Canvas(int viewportWidth, int viewportHeight, Drawable? parent, Action<object, EditorObject?> onContextBtnPressed, Action<int?> onSelectVariable, Action onAddNewVar, Action<int> onRemoveVar, Action<int, string> onRenameVariable, Action<int, DataType> onChangeVariableType, Action<int, object> onChangeVariableValue) : base(0, 0, viewportWidth, viewportHeight, parent)
    {
        this.onContextBtnPressed = onContextBtnPressed;

        variablePanel = new VariablePanel(10, 20, onSelectVariable, onAddNewVar, onRemoveVar, onRenameVariable, onChangeVariableType, OpenSearchMenu, this);
        inspectorPanel = new InspectorPanel(-200 - 10, 20, onRenameVariable, onChangeVariableType, onChangeVariableValue, OpenSearchMenu, this, ParentBasis.TopRight);
        demoPanel = new DemoPanel(60, 70, this);

        Engine.OnAnyPointerDown += HandleGlobalClickAway;
    }

    protected override void OnUpdate()
    {
        contextMenu?.Update();
        searchMenu?.Update();
        variablePanel.Update();
        inspectorPanel.Update();
        demoPanel.Update();
    }

    protected override void OnDraw()
    {
        variablePanel.Render();
        inspectorPanel.Render();
        contextMenu?.Render();
        searchMenu?.Render();
        demoPanel.Render();
    }

    protected override Drawable? OnChildrenHitTest(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        var hit = contextMenu?.HitTest(mouseScreenPosition, mouseWorldPosition) ?? null;
        if (hit != null) return hit;

        hit = searchMenu?.HitTest(mouseScreenPosition, mouseWorldPosition) ?? null;
        if (hit != null) return hit;

        hit = inspectorPanel.HitTest(mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = variablePanel.HitTest(mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = demoPanel.HitTest(mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        return null;
    }

    public override bool InteractionUseWorldPos()
    {
        return false;
    }

    protected override void OnDelete()
    {
        Engine.OnAnyPointerDown -= HandleGlobalClickAway;
    }

    private void HandleGlobalClickAway(PointerInteractEventData evt, EditorObject? target)
    {
        bool clickedInsideCtx = false;
        bool clickedInsideSearch = false;

        EditorObject? current = target;
        while (current != null && current is UIBase)
        {
            if (contextMenu != null && current == contextMenu) clickedInsideCtx = true;
            if (searchMenu != null && current == searchMenu) clickedInsideSearch = true;

            current = current.Parent as EditorObject;
        }

        if (contextMenu != null && !clickedInsideCtx && (evt.MouseButton == MouseButton.Left || evt.MouseButton == MouseButton.Right))
            RemoveCtxMenu();

        if (searchMenu != null && !clickedInsideSearch && (evt.MouseButton == MouseButton.Left || evt.MouseButton == MouseButton.Right))
            RemoveSearchMenu();
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        return false;
    }

    public void RemoveCtxMenu()
    {
        if (contextMenu != null)
        {
            if (IsFocusedChildOf(contextMenu))
                InteractionManager.ReleaseFocus();

            contextMenu.Delete();
            Engine.UIElements.Remove(contextMenu);
            contextMenu = null;
        }
    }

    public void RemoveSearchMenu()
    {
        if (searchMenu != null)
        {
            if (IsFocusedChildOf(searchMenu))
                InteractionManager.ReleaseFocus();

            searchMenu.Delete();
            Engine.UIElements.Remove(searchMenu);
            searchMenu = null;
        }
    }

    private static bool IsFocusedChildOf(EditorObject root)
    {
        EditorObject? cur = InteractionManager.CurrentlyFocused;
        while (cur != null)
        {
            if (cur == root) return true;
            cur = cur.Parent as EditorObject;
        }
        return false;
    }

    public void OpenSearchMenu(int x, int y, List<(string name, object payload)> items, Action<object> onItemSelected)
    {
        RemoveSearchMenu();
        
        SearchMenu sm = new(x, y, 200, 300, items, (payload) => {
            onItemSelected(payload);
            RemoveSearchMenu();
        }, null);

        Engine.UIElements.Add(sm);
        searchMenu = sm;
    }

    private void OnCtxBtnPressed(object payload, EditorObject? drawable)
    {
        onContextBtnPressed?.Invoke(payload, drawable);
        RemoveCtxMenu();
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Right)
            return false;

        RemoveCtxMenu();

        string deleteStr = "Delete";

        int posX = (int)evt.ScreenPosition.X;
        int posY = (int)evt.ScreenPosition.Y;

        if (InteractionManager.CurrentlyHit == null)
        {
            List<(string, object)> mis = [];
            foreach (var template in Engine.NodeRegistry.AllTemplates)
            {
                mis.Add((template.Name, template));
            }
            
            OpenSearchMenu(posX, posY, mis, (payload) => OnCtxBtnPressed(payload, null));
        }
        else if (InteractionManager.CurrentlyHit is NodeVisual nui)
        {
            ContextMenu cm = new(posX, posY, [(deleteStr, 0)], (button) => OnCtxBtnPressed(deleteStr, nui), null);
            Engine.UIElements.Add(cm);
            contextMenu = cm;
        }

        if (contextMenu == null)
            return false;

        return true;
    }

    public Dictionary<string, System.Text.Json.JsonElement> GetPanelsSaveData()
    {
        var result = new Dictionary<string, System.Text.Json.JsonElement>();
        var demoData = demoPanel.GetSaveData();
        result[demoPanel.PanelName] = System.Text.Json.JsonSerializer.SerializeToElement(demoData);
        return result;
    }

    public void RestorePanelsSaveData(Dictionary<string, System.Text.Json.JsonElement> panelsData)
    {
        if (panelsData == null) return;

        foreach (var (panelName, panelElement) in panelsData)
        {
            if (panelName == demoPanel.PanelName)
            {
                demoPanel.RestoreSaveData(panelElement);
            }
            else
            {
                Console.WriteLine($"Warning: Panel '{panelName}' found in save data, but no active panel was registered to restore its state.");
            }
        }
    }
}
