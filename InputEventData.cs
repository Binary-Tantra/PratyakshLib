using System.Numerics;
using Raylib_cs;

namespace RaylibNodeLibrary;

public class InputEventData
{
    //public InputActionType Action; // e.g., "Jump", "Delete", "Type"
}

public interface IInteractable
{
    public bool IsSelfInteractable();
    public Rectangle GetInteractableRect();
}

public interface IClippable
{
    public Rectangle GetScissorRect();
}

public enum InputDevice
{
    Mouse, Keyboard, Other
}

public class PointerVisitEventData : InputEventData
{
    public Vector2 ScreenPosition { get; internal set; }
    public Vector2 WorldPosition { get; internal set; }
}

public interface IPointerVisitable : IInteractable
{
    public void OnMouseEnter(PointerVisitEventData evt);
    public void OnMouseExit(PointerVisitEventData evt);
}

public class PointerInteractEventData : InputEventData
{
    public MouseButton MouseButton { get; internal set; }
    public Vector2 ScreenPosition { get; internal set; }
    public Vector2 WorldPosition { get; internal set; }
    public Vector2 ScreenDelta { get; internal set; }
    public Vector2 WorldDelta { get; internal set; }
}

// Down: Pointer Down.
// Up: Both pointer up and drag end.
// Double click: ...double click...
// DragStart: When drag is started. Different from pointer up so that deferred detect if input was drag or click and we dont set pointer down to objects who have not subscribed to pointer down, rather, they need drag start.
// Drag: .............drag...
public enum PointerEventType
{
    Down, Up, DoubleClick, DragStart, Drag
}

public interface IPointerInteractable : IInteractable
{
    public bool OnMouseDown(PointerInteractEventData evt);
    public bool OnMouseUp(PointerInteractEventData evt);
}

public class ScrollEventData : PointerInteractEventData
{
    public Vector2 MouseWheel { get; internal set; }
}

public interface IScrollable : IInteractable
{
    public bool OnScroll(ScrollEventData evt);
}

public interface IDoubleClickable : IInteractable
{
    bool OnDoubleClick(PointerInteractEventData eventData);
}

public interface IDragable : IInteractable
{
    public bool OnDragStart(PointerInteractEventData evt);
    public void OnDrag(PointerInteractEventData evt);
}

public class FocusEventData : InputEventData { }

public interface IFocusable : IInteractable
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

public interface IKeyInteractable : IInteractable
{
    public bool OnKeyDown(KeyInteractEventData kvt);
    public bool OnKeyUp(KeyInteractEventData kvt);
    void OnFocusLost();
}