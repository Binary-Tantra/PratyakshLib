using System.Numerics;
using Raylib_cs;

namespace RaylibNodeLibrary;

public interface IInteractable
{
    public Rectangle GetInteractableRect();
}

public enum InputDevice
{
    Mouse, Keyboard, Other
}

public class InputContext
{
    public bool isLMBCurrentlyHeld = false;
    public bool isRMBCurrentlyHeld = false;

    public bool wasLMBPressedOnceThisFrame = false;
    public bool wasRMBPressedOnceThisFrame = false;
    public bool wasLMBReleasedOnceThisFrame = false;
    public bool wasRMBReleasedOnceThisFrame = false;

    public bool wasRMBDownLastFrame = false;
    public bool wasLMBDownLastFrame = false;

    public Vector2 mouseScreenPosition = Vector2.Zero;
    public Vector2 mouseWorldPosition = Vector2.Zero;

    public Vector2 mouseScreenDelta;
    public Vector2 mouseWorldDelta;

    public float mouseWheel;

    public bool isMouseMoving = false;

    public List<KeyboardKey> keyboardKeysDown = [];

    public bool isCtrlDown = false;
    public bool isShiftDown = false;
}

public enum PointerEventType
{
    Down, Up, DoubleClick, Drag
}

public static class InteractionManager
{
    private static EditorObject? currentlyHit = null;
    private static EditorObject? currentlyHovered = null;
    private static EditorObject? currentPointerHolder = null;
    private static EditorObject? currentlyFocused = null;

    private class PendingMouseGesture
    {
        public MouseButton Button;
        public Vector2 StartPosition;
        public EditorObject? Target;
    }

    private static PendingMouseGesture? ambiguousGestureCache = null;
    private const float DRAG_THRESHOLD_SQUARE = 25f;

    private static double lastClickTime = -1;
    private static EditorObject? lastClickTarget = null;
    private static MouseButton lastClickButton;
    private const double DOUBLE_CLICK_WAIT_TIME = 0.3; // 300 milliseconds

    private static InputContext inputContext = new();

    public static InputContext InputContext { get => inputContext; }
    public static EditorObject? CurrentlyHit { get => currentlyHit; }
    public static EditorObject? CurrentlyHovered { get => currentlyHovered; }
    public static EditorObject? CurrentlyPointerSelected { get => currentPointerHolder; }
    public static EditorObject? CurrentlyFocused { get => currentlyFocused; }

    public static void UpdateInputContext(InputContext ipC)
    {
        if (inputContext.isLMBCurrentlyHeld && !ipC.isLMBCurrentlyHeld)
            inputContext.wasLMBDownLastFrame = true;
        else if (!inputContext.isLMBCurrentlyHeld && !ipC.isLMBCurrentlyHeld && inputContext.wasLMBDownLastFrame)
            inputContext.wasLMBDownLastFrame = false;

        if (inputContext.isRMBCurrentlyHeld && !ipC.isRMBCurrentlyHeld)
            inputContext.wasRMBDownLastFrame = true;
        else if (!inputContext.isRMBCurrentlyHeld && !ipC.isRMBCurrentlyHeld && inputContext.wasRMBDownLastFrame)
            inputContext.wasRMBDownLastFrame = false;

        inputContext.isLMBCurrentlyHeld = ipC.isLMBCurrentlyHeld;
        inputContext.isRMBCurrentlyHeld = ipC.isRMBCurrentlyHeld;

        inputContext.wasLMBPressedOnceThisFrame = ipC.wasLMBPressedOnceThisFrame;
        inputContext.wasRMBPressedOnceThisFrame = ipC.wasRMBPressedOnceThisFrame;

        inputContext.wasLMBReleasedOnceThisFrame = ipC.wasLMBReleasedOnceThisFrame;
        inputContext.wasRMBReleasedOnceThisFrame = ipC.wasRMBReleasedOnceThisFrame;

        inputContext.keyboardKeysDown = ipC.keyboardKeysDown;
        inputContext.isCtrlDown = ipC.isCtrlDown;
        inputContext.isShiftDown = ipC.isShiftDown;

        inputContext.mouseScreenDelta = ipC.mouseScreenPosition - inputContext.mouseScreenPosition;
        inputContext.mouseWorldDelta = ipC.mouseWorldPosition - inputContext.mouseWorldPosition;

        inputContext.mouseWheel = ipC.mouseWheel;

        inputContext.mouseScreenPosition = ipC.mouseScreenPosition;
        inputContext.mouseWorldPosition = ipC.mouseWorldPosition;

        inputContext.isMouseMoving = inputContext.mouseScreenDelta.LengthSquared() > 0;
    }

    private static EditorObject? TryPointerVisitation(EditorObject? newHoveredInteractableEO)
    {
        bool wasCompleted = false;

        while (newHoveredInteractableEO != null)
        {
            if (newHoveredInteractableEO is IPointerVisitable visitedIt)
            {
                if (currentlyHovered != null)
                {
                    PointerVisitEventData oldPvevt = new()
                    {
                        ScreenPosition = inputContext.mouseScreenPosition,
                        WorldPosition = inputContext.mouseWorldPosition
                    };

                    (currentlyHovered as IPointerVisitable)?.OnMouseExit(oldPvevt);
                }

                PointerVisitEventData potentialPvevt = new()
                {
                    ScreenPosition = inputContext.mouseScreenPosition,
                    WorldPosition = inputContext.mouseWorldPosition
                };

                visitedIt.OnMouseEnter(potentialPvevt);
                return newHoveredInteractableEO;
            }
            else newHoveredInteractableEO = newHoveredInteractableEO.Parent != null ? newHoveredInteractableEO.Parent as EditorObject : null;
        }

        if (!wasCompleted)
        {
            if (currentlyHovered != null)
            {
                PointerVisitEventData oldPvevt = new()
                {
                    ScreenPosition = inputContext.mouseScreenPosition,
                    WorldPosition = inputContext.mouseWorldPosition
                };

                (currentlyHovered as IPointerVisitable)?.OnMouseExit(oldPvevt);
            }
        }

        return null;
    }

    public static void CapturePointer(EditorObject target)
    {
        if (currentPointerHolder != null)
            Console.WriteLine("Warning: Current EditorObject Pointer holder was not null, but a new EditorObject now holds the pointer. Old: " + currentPointerHolder + ". New: " + target + ".");

        currentPointerHolder = target;
    }

    public static void ReleasePointer()
    {
        currentPointerHolder = null;
    }

    public static void CaptureFocus(EditorObject target)
    {
        currentlyFocused = target;
    }

    public static void ReleaseFocus()
    {
        currentlyFocused = null;
    }

    public static Drawable? FindDeepestHitObject(Vector2 mouseScreenPos, Vector2 mouseWorldPos)
    {
        for (int i = Engine.UIElements.Count - 1; i >= 0; i--)
        {
            Drawable? hit = Engine.UIElements[i].HitTest(mouseScreenPos, mouseWorldPos);

            if (hit != null)
                return hit;
        }

        for (int i = Engine.Actors.Count - 1; i >= 0; i--)
        {
            Drawable? hit = Engine.Actors[i].HitTest(mouseScreenPos, mouseWorldPos);

            if (hit != null)
                return hit;
        }

        return null;
    }

    public static bool DispatchPointer(EditorObject? clickedEO, PointerInteractEventData eventData, PointerEventType pointerEventType, bool bubble = true)
    {
        while (clickedEO != null)
        {
            bool checkCompleted = true;

            if (clickedEO is IPointerInteractable handler)
            {
                if (pointerEventType == PointerEventType.Down)
                {
                    if (handler.OnMouseDown(eventData))
                        return true;
                }
                else if (pointerEventType == PointerEventType.Up)
                {
                    if (handler.OnMouseUp(eventData))
                        return true;
                }
                else checkCompleted = false;
            }
            
            if (!checkCompleted && clickedEO is IScrollable scrollHandler)
            {
                if ((pointerEventType == PointerEventType.Up || pointerEventType == PointerEventType.Down) && eventData is ScrollEventData sed)
                {
                    if (scrollHandler.OnScroll(sed))
                        return true;
                }
                else checkCompleted = false;
            }

            if (!checkCompleted && clickedEO is IDoubleClickable doubleClickHandler)
            {
                if (pointerEventType == PointerEventType.DoubleClick)
                {
                    if (doubleClickHandler.OnDoubleClick(eventData))
                        return true;
                }
                else checkCompleted = false;
            }
            
            if (!checkCompleted && clickedEO is IDragable dragHandler)
            {
                if (pointerEventType == PointerEventType.Drag)
                {
                    dragHandler.OnDrag(eventData);
                    return true;
                }
                else checkCompleted = false;
            }

            if (bubble) clickedEO = (clickedEO.Parent is EditorObject) ? clickedEO.Parent as EditorObject : null;
            else clickedEO = null;
        }

        if (bubble && clickedEO == null)
        {
            Engine.HandleGlobalPointerEvent(eventData, pointerEventType);
        }

        return false;
    }

    private static void HandleKBInput()
    {
        IKeyInteractable? keyable = (IKeyInteractable?)currentlyFocused;

        for (int i = 0; i < inputContext.keyboardKeysDown.Count; i++)
        {
            KeyInteractEventData keyEvent = new()
            {
                Key = inputContext.keyboardKeysDown[i],
                IsCtrlDown = inputContext.isCtrlDown,
                IsShiftDown = inputContext.isShiftDown
            };

            if (keyable != null && keyable.OnKeyDown(keyEvent))
                continue;
            else Engine.HandleGlobalKBEvents(keyEvent);
        }
    }

    private static bool TryDoubleClick(MouseButton pressedButton, EditorObject? currentTarget)
    {
        bool isFastEnough = (Raylib.GetTime() - lastClickTime) < DOUBLE_CLICK_WAIT_TIME;
        bool isSameTarget = currentTarget == lastClickTarget;
        bool isSameButton = pressedButton == lastClickButton;

        if (isFastEnough && isSameTarget && isSameButton)
        {
            PointerInteractEventData dcEvt = new()
            {
                ScreenPosition = inputContext.mouseScreenPosition,
                WorldPosition = inputContext.mouseWorldPosition,
                ScreenDelta = inputContext.mouseScreenDelta,
                WorldDelta = inputContext.mouseWorldDelta,
                mouseButton = pressedButton,
                IsDragRelated = false
            };

            DispatchPointer(currentTarget, dcEvt, PointerEventType.DoubleClick);

            lastClickTime = -1;
            lastClickTarget = null;

            return true;
        }

        return false;
    }

    private static bool TryResolveDragOrClick(PendingMouseGesture pendingGesture, bool isLMBDown, bool isRMBDown, bool wasLMBReleased, bool wasRMBReleased)
    {
        bool isStillDown = (pendingGesture.Button == MouseButton.Left && isLMBDown) ||
                          (pendingGesture.Button == MouseButton.Right && isRMBDown);

        bool isNowReleased = (pendingGesture.Button == MouseButton.Left && wasLMBReleased) ||
                          (pendingGesture.Button == MouseButton.Right && wasRMBReleased);

        if (!isStillDown && !isNowReleased) // Can happen due to frame hiccups (release frame can be missed and down is also false)
        {
            ReleasePointer(); // Force release pointer.
            return true; // Force resolve.
        }

        if (isStillDown)
        {
            float distSq = Vector2.DistanceSquared(pendingGesture.StartPosition, inputContext.mouseScreenPosition);
            if (distSq > DRAG_THRESHOLD_SQUARE)
            {
                // Resolved as DRAG
                PointerInteractEventData downEvt = new()
                {
                    ScreenPosition = inputContext.mouseScreenPosition,
                    WorldPosition = inputContext.mouseWorldPosition,
                    ScreenDelta = inputContext.mouseScreenDelta,
                    WorldDelta = inputContext.mouseWorldDelta,
                    mouseButton = pendingGesture.Button,
                    IsDragRelated = true
                };

                // Fire the delayed MouseDown since drag is confirmed
                DispatchPointer(pendingGesture.Target, downEvt, PointerEventType.Down);

                return true;
            }
        }
        else if (isNowReleased)
        {
            // Resolved as CLICK
            PointerInteractEventData evt = new()
            {
                ScreenPosition = inputContext.mouseScreenPosition,
                WorldPosition = inputContext.mouseWorldPosition,
                ScreenDelta = inputContext.mouseScreenDelta,
                WorldDelta = inputContext.mouseWorldDelta,
                mouseButton = pendingGesture.Button,
                IsDragRelated = false
            };

            DispatchPointer(pendingGesture.Target, evt, PointerEventType.Down);
            DispatchPointer(pendingGesture.Target, evt, PointerEventType.Up); // Should we use currently hovered for up? Because underlying object could have changed between down and up! But then pendingGesture.Target would not receive up!

            // Record this click to test for potential Double-Clicks in later frames
            lastClickTime = Raylib.GetTime();
            lastClickTarget = pendingGesture.Target;
            lastClickButton = pendingGesture.Button;

            return true;
        }

        return false;
    }

    private static void HandleCurrentPointerHolder(bool wasPressedOrReleased, bool wasLMBPressed, bool wasRMBPressed, bool wasLMBReleased, bool wasRMBReleased, EditorObject pointerSelected, float mouseWheel)
    {
        if (pointerSelected is IDragable && inputContext.isMouseMoving)
        {
            PointerInteractEventData dragPied = new()
            {
                ScreenPosition = inputContext.mouseScreenPosition,
                WorldPosition = inputContext.mouseWorldPosition,
                ScreenDelta = inputContext.mouseScreenDelta,
                WorldDelta = inputContext.mouseWorldDelta,
                IsDragRelated = true
            };

            DispatchPointer(pointerSelected, dragPied, PointerEventType.Drag);
        }

        PointerInteractEventData potentialPied = new()
        {
            ScreenPosition = inputContext.mouseScreenPosition,
            WorldPosition = inputContext.mouseWorldPosition,
            ScreenDelta = inputContext.mouseScreenDelta,
            WorldDelta = inputContext.mouseWorldDelta,
            IsDragRelated = false
        };

        if (wasLMBPressed)
        {
            potentialPied.mouseButton = MouseButton.Left;
            DispatchPointer(pointerSelected, potentialPied, PointerEventType.Down);
        }

        if (wasLMBReleased)
        {
            potentialPied.mouseButton = MouseButton.Left;
            DispatchPointer(pointerSelected, potentialPied, PointerEventType.Up);
        }

        if (wasRMBPressed)
        {
            potentialPied.mouseButton = MouseButton.Right;
            DispatchPointer(pointerSelected, potentialPied, PointerEventType.Down);
        }

        if (wasRMBReleased)
        {
            potentialPied.mouseButton = MouseButton.Right;
            DispatchPointer(pointerSelected, potentialPied, PointerEventType.Up);
        }

        if (mouseWheel != 0)
        {
            ScrollEventData potentialSed = new()
            {
                ScreenPosition = inputContext.mouseScreenPosition,
                WorldPosition = inputContext.mouseWorldPosition,
                ScreenDelta = inputContext.mouseScreenDelta,
                WorldDelta = inputContext.mouseWorldDelta,
                mouseButton = MouseButton.Middle,
                IsDragRelated = false,
                MouseWheel = new Vector2(0, mouseWheel)
            };

            DispatchPointer(currentlyHovered, potentialSed, mouseWheel < 0 ? PointerEventType.Down : PointerEventType.Up);
        }
    }

    private static void HandleUnambiguousInput(EditorObject? currentlyHovered, bool wasLMBPressed, bool wasLMBReleased, bool wasRMBPressed, bool wasRMBReleased, float mouseWheel)
    {
        PointerInteractEventData potentialPied;

        if (currentlyHovered != null)
        {
            potentialPied = new()
            {
                ScreenPosition = inputContext.mouseScreenPosition,
                WorldPosition = inputContext.mouseWorldPosition,
                ScreenDelta = inputContext.mouseScreenDelta,
                WorldDelta = inputContext.mouseWorldDelta,
                IsDragRelated = false
            };
        }
        else
        {
            potentialPied = new()
            {
                ScreenPosition = inputContext.mouseScreenPosition,
                WorldPosition = inputContext.mouseWorldPosition,
                ScreenDelta = inputContext.mouseScreenDelta,
                WorldDelta = inputContext.mouseWorldDelta,
                IsDragRelated = false
            };
        }

        if (wasLMBPressed)
        {
            potentialPied.mouseButton = MouseButton.Left;
            DispatchPointer(currentlyHovered, potentialPied, PointerEventType.Down);
        }

        if (wasLMBReleased)
        {
            potentialPied.mouseButton = MouseButton.Left;
            DispatchPointer(currentlyHovered, potentialPied, PointerEventType.Up);
        }

        if (wasRMBPressed)
        {
            potentialPied.mouseButton = MouseButton.Right;
            DispatchPointer(currentlyHovered, potentialPied, PointerEventType.Down);
        }

        if (wasRMBReleased)
        {
            potentialPied.mouseButton = MouseButton.Right;
            DispatchPointer(currentlyHovered, potentialPied, PointerEventType.Up);
        }

        if (mouseWheel != 0)
        {
            ScrollEventData potentialSed = new()
            {
                ScreenPosition = inputContext.mouseScreenPosition,
                WorldPosition = inputContext.mouseWorldPosition,
                ScreenDelta = inputContext.mouseScreenDelta,
                WorldDelta = inputContext.mouseWorldDelta,
                mouseButton = MouseButton.Middle,
                IsDragRelated = false,
                MouseWheel = new Vector2(0, mouseWheel)
            };

            DispatchPointer(currentlyHovered, potentialSed, mouseWheel < 0 ? PointerEventType.Down : PointerEventType.Up);
        }
    }

    public static void HandleInput()
    {
        HandleKBInput();

        bool wasLMBPressed = inputContext.wasLMBPressedOnceThisFrame;
        bool wasRMBPressed = inputContext.wasRMBPressedOnceThisFrame;

        // For now, if both pressed, precedence to left.
        MouseButton? mousePressed = (wasLMBPressed || wasRMBPressed) ? (wasLMBPressed ? MouseButton.Left : MouseButton.Right) : null;

        bool wasLMBReleased = inputContext.wasLMBReleasedOnceThisFrame;
        bool wasRMBReleased = inputContext.wasRMBReleasedOnceThisFrame;

        // For now, if both released, precedence to left.
        MouseButton? mouseReleased = (wasLMBReleased || wasRMBReleased) ? (wasLMBReleased ? MouseButton.Left : MouseButton.Right) : null;

        bool isLMBDown = inputContext.isLMBCurrentlyHeld;
        bool isRMBDown = inputContext.isRMBCurrentlyHeld;

        bool wasPressedOrReleased = wasLMBPressed || wasRMBPressed || wasLMBReleased || wasRMBReleased;

        float mouseWheel = inputContext.mouseWheel;

        Vector2 screenPos = inputContext.mouseScreenPosition;
        Vector2 worldPos = inputContext.mouseWorldPosition;

        Drawable? currentHitDrawable = FindDeepestHitObject(screenPos, worldPos);
        currentlyHit = (EditorObject?)currentHitDrawable;

        if (currentlyHit != currentlyHovered)
            currentlyHovered = TryPointerVisitation(currentlyHit);

        if (mousePressed.HasValue && ambiguousGestureCache == null)
        {
            if (!TryDoubleClick(mousePressed.Value, currentlyHit))
            {
                if (currentlyFocused != null && currentlyHit != currentlyFocused)
                {
                    if (currentlyFocused is IKeyInteractable oldFocus)
                        oldFocus.OnFocusLost();

                    ReleaseFocus();
                }

                PointerInteractEventData globalDownEvt = new()
                {
                    ScreenPosition = inputContext.mouseScreenPosition,
                    WorldPosition = inputContext.mouseWorldPosition,
                    mouseButton = mousePressed.Value,
                    IsDragRelated = false // Drag will always be false for global pointer down event.
                };

                Engine.NotifyAnyPointerDown(globalDownEvt, currentlyHit);

                // If not double click, create new ambiguous gesture.
                ambiguousGestureCache = new PendingMouseGesture
                {
                    Button = mousePressed.Value,
                    StartPosition = inputContext.mouseScreenPosition,
                    Target = currentlyHit
                };
            }

            return;
        }

        if (ambiguousGestureCache != null)
        {
            if (TryResolveDragOrClick(ambiguousGestureCache, isLMBDown, isRMBDown, wasLMBReleased, wasRMBReleased))
            {
                ambiguousGestureCache = null;
                return;
            }
        }

        if (ambiguousGestureCache == null)
        {
            if (currentPointerHolder != null)
            {
                HandleCurrentPointerHolder(wasPressedOrReleased, wasLMBPressed, wasRMBPressed, wasLMBReleased, wasRMBReleased, currentPointerHolder, mouseWheel);
            }
            else HandleUnambiguousInput(currentlyHovered, wasLMBPressed, wasLMBReleased, wasRMBPressed, wasRMBReleased, mouseWheel);
        }
    }
}
