using Pratyaksh.Core;
using System.Numerics;

namespace Pratyaksh.UI;

internal interface IOverlayable
{
    public void DrawOverlay();
}

public abstract class UILayoutBase : UIBase, IPointerInteractable, IDragable, IClippable
{
    protected LayoutEngine layout;

    private bool isDragging = false;
    private Vector2 dragOffset;

    protected int mainVerticalSpacing = 0;
    protected int horizontalPadding = 0;
    protected int verticalBgOffset = 0;
    protected int verticalDrawStopOffset = 0;

    public string PanelSaveName => PanelName;

    protected abstract string PanelName { get; }

    protected int ContentWidth { get => UITransform.Width - horizontalPadding; }
    protected int ContentHeight { get => UITransform.Height - verticalBgOffset - verticalDrawStopOffset; }
    protected int RemainingWidth { get => Math.Clamp(ContentWidth - layout.PosXRelative(), 0, ContentWidth); }
    protected int RemainingHeight { get => Math.Clamp(ContentHeight - layout.PosYRelative() + verticalBgOffset, 0, UITransform.Height); }

    public UILayoutBase(int posX, int posY, int layoutWidth, int layoutHeight, Transform? parent, ParentBasis? parentBasis = null) : base(posX, posY, layoutWidth, layoutHeight, parentBasis, parent)
    {
        selfInteractable = true;
        layout = new LayoutEngine(this);
    }

    protected override void OnDraw()
    {
        bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();
        Rectangle finalRect = new(UITransform.X, UITransform.Y, UITransform.Width, UITransform.Height);

        if (worldSpace)
            finalRect = Engine.Instance.InteractionManager.WorldToScreenTransformer.WorldToScreen(finalRect);

        layout.BeginFrame();

        Raylib_cs.Raylib.BeginScissorMode((int)finalRect.X, (int)finalRect.Y, (int)finalRect.Width, (int)finalRect.Height);
        {
            layout.BeginHorizontalEx(0, (int)UITransform.X);
            {
                layout.AddSpace(horizontalPadding);

                layout.BeginVerticalEx(mainVerticalSpacing, (int)UITransform.Y);
                {
                    OnDrawLayout();
                }
                layout.EndVertical(UITransform.Width);
            }
            layout.EndHorizontal(UITransform.Height);
        }
        Raylib_cs.Raylib.EndScissorMode();

        layout.DrawOverlays();
        layout.EndFrame();
    }

    protected override void OnUpdate()
    {
        layout.UpdateLayoutElements();
    }

    protected override void OnDelete()
    {
        layout.ResetLayout();
    }

    public abstract void OnDrawLayout();

    protected override PratyakshObject? OnChildrenHitTest(IWorldToScreenTransformer2D transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        return layout.HitTestElements(transformer, mouseScreenPosition, mouseWorldPosition);
    }

    public Rectangle GetScissorRect(IWorldToScreenTransformer2D worldToScreenTransformer)
    {
        Rectangle rect = new(UITransform.X, UITransform.Y, UITransform.Width, UITransform.Height);
        bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();

        if (worldSpace)
            rect = worldToScreenTransformer.WorldToScreen(rect);

        return rect;
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        return false;
    }

    public bool OnDragStart(PointerInteractEventData evt)
    {
        if (evt.MouseButton == MouseButton.Left)
        {
            isDragging = true;
            dragOffset = new Vector2(evt.ScreenPosition.X - UITransform.RelX, evt.ScreenPosition.Y - UITransform.RelY);

            Engine.Instance.InteractionManager.CapturePointer(this);

            return true;
        }

        return false;
    }

    public void OnDrag(PointerInteractEventData evt)
    {
        if (isDragging)
            UITransform.RelativePosition = new Vector3(evt.ScreenPosition.X - dragOffset.X, evt.ScreenPosition.Y - dragOffset.Y, 0);
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton == MouseButton.Left)
        {
            isDragging = false;
            Engine.Instance.InteractionManager.ReleasePointer();

            return true;
        }

        return false;
    }

    public virtual Dictionary<string, object?> GetSaveData() => [];

    public virtual void RestoreSaveData(System.Text.Json.JsonElement data) { }
}
