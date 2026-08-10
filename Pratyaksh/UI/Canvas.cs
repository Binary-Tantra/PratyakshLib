using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI;

public class Canvas : UIBase, IPointerInteractable
{
    private List<(UILayoutBase? panelRef, bool saveable, bool transient)> panels = [];
    private Func<PointerInteractEventData, EditorObject?, bool> onContextClickReceived;

    public Canvas(int viewportWidth, int viewportHeight, Func<PointerInteractEventData, EditorObject?, bool> onContextClickReceived) : base(0, 0, viewportWidth, viewportHeight, null)
    {
        this.onContextClickReceived = onContextClickReceived;
        Engine.Instance.InteractionManager.AnyPointerEvent += HandleGlobalClickAway;
    }

    public int AddPanel(UILayoutBase? panel, bool saveable, bool transient)
    {
        panels.Add((panel, saveable, transient));
        return panels.Count - 1;
    }

    protected override void OnUpdate()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].panelRef?.Update();
        }
    }

    protected override void OnDraw()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].panelRef?.Render();
        }
    }

    protected override Drawable? OnChildrenHitTest(IWorldToScreenTransformer transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        for (int i = panels.Count - 1; i >= 0; i--)
        {
            var hit = panels[i].panelRef?.HitTest(transformer, mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        return null;
    }

    public override bool InteractionUseWorldPos()
    {
        return false;
    }

    protected override void OnDelete()
    {
        Engine.Instance.InteractionManager.AnyPointerEvent -= HandleGlobalClickAway;
    }

    private void HandleGlobalClickAway(PointerInteractEventData evt, EditorObject? target)
    {
        if (target == null)
            return;

        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i].transient && panels[i].panelRef != null)
            {
                bool clickedInsidePanel = target.IsAncestor(panels[i].panelRef!);

                if (panels[i].panelRef != null && !clickedInsidePanel && (evt.MouseButton == MouseButton.Left || evt.MouseButton == MouseButton.Right))
                    CloseTransPanel(i);
            }
        }

        
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        return false;
    }

    public T? OpenTransPanel<T>(int index, params object?[]? args) where T : UILayoutBase
    {
        CloseTransPanel(index);

        if (Activator.CreateInstance(typeof(T), args) is not T panel)
        {
            Console.WriteLine("Error: Failed to create instance of panel type " + typeof(T).Name);
            return null;
        }

        panels[index] = (panel, panels[index].saveable, panels[index].transient);

        return panel;
    }

    private void CloseAllTransPanels()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i].transient)
                CloseTransPanel(i);
        }
    }

    public void CloseTransPanel(int index)
    {
        UILayoutBase? panel = panels[index].panelRef;

        if (panel == null)
            return;

        if (IsAnyChildFocused(panel))
            Engine.Instance.InteractionManager.ReleaseFocus();

        panel.Delete();
        panels[index] = (null, panels[index].saveable, panels[index].transient);
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Right)
            return false;

        CloseAllTransPanels();

        bool? result = onContextClickReceived?.Invoke(evt, Engine.Instance.InteractionManager.CurrentlyHit);

        return result ?? false;
    }

    public Dictionary<string, System.Text.Json.JsonElement> GetPanelsSaveData()
    {
        var result = new Dictionary<string, System.Text.Json.JsonElement>();
        
        for (int i = 0; i < panels.Count; i++)
        {
            (UILayoutBase? panelRef, bool saveable, _) = panels[i];

            if (!saveable || panelRef == null)
                continue;

            var panelData = panelRef.GetSaveData();
            result[panelRef.PanelSaveName] = System.Text.Json.JsonSerializer.SerializeToElement(panelData);
        }

        return result;
    }

    public void RestorePanelsSaveData(Dictionary<string, System.Text.Json.JsonElement> panelsData)
    {
        if (panelsData == null)
            return;

        for (int i = 0; i < panels.Count; i++)
        {
            (UILayoutBase? panelRef, bool saveable, _) = panels[i];

            if (!saveable || panelRef == null)
                continue;

            if (panelsData.TryGetValue(panelRef.PanelSaveName, out var panelElement))
            {
                panelRef.RestoreSaveData(panelElement);
            }
            else Console.WriteLine($"Warning: No save data found for panel '{panelRef.PanelSaveName}'.");
        }
    }
}
