/*using Raylib_cs;
using System.Numerics;

namespace RaylibNodeLibrary;

public interface IPointerVisitable
{
    public void OnMouseEnter();
    public void OnMouseExit();
    public Rectangle GetMouseInteractableRect();
    public bool UseWorldSpacePositionsMouseInteractable();
}

public class InputAction
{
    public InputDevice device;

    public InputAction(InputDevice device)
    {
        this.device = device;
    }
}

public class MouseInputAction : InputAction
{
    public MouseButton button;

    public MouseInputAction(MouseButton button) : base(InputDevice.Mouse)
    {
        this.button = button;
    }
}

public class KeyboardInputAction : InputAction
{
    public KeyboardKey key;

    public KeyboardInputAction(KeyboardKey key) : base(InputDevice.Keyboard)
    {
        this.key = key;
    }
}

public interface IInteractable
{
    public List<InputAction> GetInputActions();
    public int GetInteractablePriority();
    public bool UseWorldSpacePositions();
    public Rectangle GetInteractableRect();
    public bool IsInteractionUpdatable();
    public bool DoesInteractionBlockOthers();
    public void OnInteractionStart(InputContext inputContext, InputAction action);
    public void OnInteractionUpdate(InputContext inputContext, InputAction action);
    public void OnInteractionEnd(InputContext inputContext, InputAction action);
}

public enum InputDevice
{
    Mouse, Keyboard, Other
}

public class InputContext
{
    public bool isLMBDown = false;
    public bool isRMBDown = false;
    public bool isLMBUp = false;
    public bool isRMBUp = false;
    public bool wasLMBPressedOnce = false;
    public bool wasRMBPressedOnce = false;
    public bool wasLMBReleasedOnce = false;
    public bool wasRMBReleasedOnce = false;

    public bool wasRMBDown = false;
    public bool wasLMBDown = false;

    public bool isMouseDragging = false;

    public Vector2 mousePosition = Vector2.Zero;
    public Vector2 mouseWorldPosition = Vector2.Zero;

    public IInteractable? hoverDrawable;

    public List<KeyboardKey> keyboardKeysDown = [];
}

public static class InteractionManager
{
    private static Dictionary<IInteractable, List<InputAction>> interactables = [];
    private static List<(IInteractable interactable, InputAction action)> activeInteractions = new();
    private static bool activatedInteractions = false;

    private static Vector2 interactionStartMousePos;
    private static bool interactionDragging = false;

    private const float dragThreshold = 0.05f;

    private static InputContext inputContext = new();

    private static Dictionary<IPointerVisitable, bool> pointerInteractables = []; // bool is if it is currently hovered

    public static InputContext InputContext { get => inputContext; }

    public static IInteractable? FirstActiveInteractable { get => activeInteractions[0].interactable; }

    public static void RegisterPointerInteractable(IPointerVisitable newInteractable)
    {
        pointerInteractables.Add(newInteractable, false);
    }

    public static void UnregisterPointerInteractable(IPointerVisitable interactable)
    {
        pointerInteractables.Remove(interactable);
    }

    public static void RegisterInteractable(IInteractable newInteractable, InputAction action)
    {
        if (interactables.TryGetValue(newInteractable, out List<InputAction>? actions))
        {
            actions ??= [];
            actions.Add(action);
            interactables[newInteractable] = actions;
        }
        else interactables.Add(newInteractable, [action]);
    }

    public static void UnregisterInteractable(IInteractable interactable)
    {
        interactables.Remove(interactable);
    }

    public static void UpdateInputContext(InputContext ipC)
    {
        if (inputContext.isLMBDown && !ipC.isLMBDown)
            inputContext.wasLMBDown = true;
        else if (!inputContext.isLMBDown && !ipC.isLMBDown && inputContext.wasLMBDown)
            inputContext.wasLMBDown = false;

        if (inputContext.isRMBDown && !ipC.isRMBDown)
            inputContext.wasRMBDown = true;
        else if (!inputContext.isRMBDown && !ipC.isRMBDown && inputContext.wasRMBDown)
            inputContext.wasRMBDown = false;

        inputContext.isLMBDown = ipC.isLMBDown;
        inputContext.isRMBDown = ipC.isRMBDown;

        inputContext.isLMBUp = ipC.isLMBUp;
        inputContext.isRMBUp = ipC.isRMBUp;

        inputContext.wasLMBPressedOnce = ipC.wasLMBPressedOnce;
        inputContext.wasRMBPressedOnce = ipC.wasRMBPressedOnce;

        inputContext.wasLMBReleasedOnce = ipC.wasLMBReleasedOnce;
        inputContext.wasRMBReleasedOnce = ipC.wasRMBReleasedOnce;

        inputContext.mousePosition = ipC.mousePosition;
        inputContext.mouseWorldPosition = ipC.mouseWorldPosition;

        inputContext.keyboardKeysDown = ipC.keyboardKeysDown;
    }

    public static List<(IInteractable interactable, InputAction targetInputAction)> GetPotentialInteractionTargets(InputDevice device, InputAction? performedAction)
    {
        List<(IInteractable interactable, InputAction targetInputAction)> interactionTargets = [];
        List<IInteractable> its = [.. interactables.Keys];

        for (int i = 0; i < its.Count; i++)
        {
            List<InputAction> registeredActions = its[i].GetInputActions();

            for (int j = 0; j < registeredActions.Count; j++)
            {
                if (registeredActions[j].device == device && (performedAction == null || performedAction.GetType() == registeredActions[j].GetType()))
                {
                    if (device == InputDevice.Mouse)
                    {
                        MouseInputAction mia = (MouseInputAction)registeredActions[j];
                        MouseInputAction mia2 = (MouseInputAction)(performedAction ?? mia);

                        if (mia.button == mia2.button)
                        {
                            Rectangle interactableRect = its[i].GetInteractableRect();

                            if (interactableRect.Width == float.NegativeInfinity)
                                interactionTargets.Add((its[i], registeredActions[j]));
                            else
                            {
                                if (its[i].UseWorldSpacePositions())
                                {
                                    if (Raylib.CheckCollisionPointRec(inputContext.mouseWorldPosition, interactableRect))
                                        interactionTargets.Add((its[i], registeredActions[j]));
                                }
                                else
                                {
                                    if (Raylib.CheckCollisionPointRec(inputContext.mousePosition, interactableRect))
                                        interactionTargets.Add((its[i], registeredActions[j]));
                                }
                            }
                        }
                    }
                    
                    if (device == InputDevice.Keyboard)
                    {
                        KeyboardInputAction kia = (KeyboardInputAction)registeredActions[j];
                        KeyboardInputAction kia2 = (KeyboardInputAction)(performedAction ?? kia);

                        if (kia.key == kia2.key)
                            interactionTargets.Add((its[i], registeredActions[j]));
                    }
                }
            }
        }

        if (interactionTargets.Count > 1)
            interactionTargets.Sort((left, right) => right.interactable.GetInteractablePriority().CompareTo(left.interactable.GetInteractablePriority()));

        return interactionTargets;
    }

    private static void HandlePointerInteractables()
    {
        List<KeyValuePair<IPointerVisitable, bool>> pIts = [.. pointerInteractables];

        for (int i = 0; i < pIts.Count; i++)
        {
            Vector2 collisionPoint;
            if (pIts[i].Key.UseWorldSpacePositionsMouseInteractable())
                collisionPoint = inputContext.mouseWorldPosition;
            else
                collisionPoint = inputContext.mousePosition;

            if (Raylib.CheckCollisionPointRec(collisionPoint, pIts[i].Key.GetMouseInteractableRect()))
            {
                if (!pIts[i].Value)
                {
                    pointerInteractables[pIts[i].Key] = true;
                    pIts[i].Key.OnMouseEnter();
                }
            }
            else
            {
                if (pIts[i].Value)
                {
                    pointerInteractables[pIts[i].Key] = false;
                    pIts[i].Key.OnMouseExit();
                }
            }
        }
    }

    private static void AddActiveInteractables(InputAction action)
    {
        List<(IInteractable interactable, InputAction targetInputAction)> potentialInteractables = GetPotentialInteractionTargets(action.device, action);

        for (int i = 0; i < potentialInteractables.Count; i++)
        {
            activeInteractions.Add((potentialInteractables[i].interactable, action));

            interactionStartMousePos = inputContext.mousePosition;
            interactionDragging = false;

            activeInteractions[^1].interactable.OnInteractionStart(inputContext, action);

            if (activeInteractions[^1].interactable.DoesInteractionBlockOthers())
                break;
        }

        activatedInteractions = activeInteractions.Count > 0;
    }

    public static void HandleInput()
    {
        List<(IInteractable interactable, InputAction targetInputAction)> potentialHovers = GetPotentialInteractionTargets(InputDevice.Mouse, null);

        if (potentialHovers.Count > 1)
            inputContext.hoverDrawable = potentialHovers[0].interactable;
        else inputContext.hoverDrawable = null;

        HandlePointerInteractables();

        for (int i = 0; i < activeInteractions.Count; i++)
            activeInteractions[i].interactable.OnInteractionUpdate(inputContext, activeInteractions[i].action);

        if (!activatedInteractions)
        {
            if (inputContext.isLMBDown)
                AddActiveInteractables(new MouseInputAction(MouseButton.Left));

            if (inputContext.isRMBDown)
                AddActiveInteractables(new MouseInputAction(MouseButton.Right));

            if (inputContext.keyboardKeysDown.Count > 0)
            {
                for (int i = 0; i < inputContext.keyboardKeysDown.Count; i++)
                    AddActiveInteractables(new KeyboardInputAction(inputContext.keyboardKeysDown[i]));
            }
        }
        else
        {
            if (!interactionDragging)
            {
                float dist = Vector2.Distance(interactionStartMousePos, inputContext.mousePosition);

                if (dist >= dragThreshold)
                    interactionDragging = true;
            }

            inputContext.isMouseDragging = interactionDragging;
            
            List<bool> ended = [];
            for (int i = 0; i < activeInteractions.Count; i++)
            {
                if (activeInteractions[i].action.device == InputDevice.Mouse)
                {
                    MouseInputAction mia = (MouseInputAction)activeInteractions[i].action;

                    if ((mia.button == MouseButton.Left && inputContext.isLMBUp && inputContext.wasLMBDown) ||
                        (mia.button == MouseButton.Right && inputContext.isRMBUp && inputContext.wasRMBDown))
                    {
                        activeInteractions[i].interactable.OnInteractionEnd(inputContext, activeInteractions[i].action);
                        ended.Add(true);
                    }
                    else ended.Add(false);
                }
            }

            List<(IInteractable interactable, InputAction action)> newActiveInteractions = new();

            for (int i = 0; i < ended.Count; i++)
            {
                if (!ended[i])
                    newActiveInteractions.Add(activeInteractions[i]);
            }

            activeInteractions = newActiveInteractions;

            if (activeInteractions.Count == 0)
            {
                inputContext.isMouseDragging = false;
                activatedInteractions = false;
            }
        }
    }
}
*/