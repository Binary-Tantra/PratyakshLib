using System.Numerics;
using Raylib_cs;

namespace RaylibNodeLibrary;

public class InputEventData
{
    //public InputActionType Action; // e.g., "Jump", "Delete", "Type"
}

public class PointerVisitEventData : InputEventData
{
    public Vector2 ScreenPosition { get; internal set; }
    public Vector2 WorldPosition { get; internal set; }
}

public interface IPointerVisitable
{
    public void OnMouseEnter(PointerVisitEventData evt);
    public void OnMouseExit(PointerVisitEventData evt);
}

public class PointerInteractEventData : InputEventData
{
    public MouseButton mouseButton { get; internal set; }
    public Vector2 ScreenPosition { get; internal set; }
    public Vector2 WorldPosition { get; internal set; }
    public Vector2 ScreenDelta { get; internal set; }
    public Vector2 WorldDelta { get; internal set; }
    public bool IsDragRelated { get; internal set; }
}

public interface IPointerInteractable
{
    public bool OnMouseDown(PointerInteractEventData evt);
    public bool OnMouseUp(PointerInteractEventData evt);
}

public class ScrollEventData : PointerInteractEventData
{
    public Vector2 MouseWheel { get; internal set; }
}

public interface IScrollable
{
    public bool OnScroll(ScrollEventData evt);
}

public interface IDoubleClickable
{
    bool OnDoubleClick(PointerInteractEventData eventData);
}

public interface IDragable
{
    public void OnDrag(PointerInteractEventData evt);
}

public class FocusEventData : InputEventData { }

public interface IFocusable
{
    public bool OnFocus(FocusEventData evt);
    public bool OnUnfocus(FocusEventData evt);
}

public class KeyInteractEventData : InputEventData
{
    public KeyboardKey Key { get; internal set; }
    public bool IsCtrlDown { get; internal set; }
    public bool IsShiftDown { get; internal set; }
}

public interface IKeyInteractable
{
    public bool OnKeyDown(KeyInteractEventData kvt);
    public bool OnKeyUp(KeyInteractEventData kvt);
    void OnFocusLost();
}