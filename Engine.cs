namespace RaylibNodeLibrary;

using System.Numerics;
using Raylib_cs;
using RaylibNodeLibrary.DataModel;
using RaylibNodeLibrary.UI;

public class Engine
{
    private static int screenWidth;
    private static int screenHeight;

    private static EditorCamera2D camera;
    private static Canvas canvas;

    private static ConnectionVisualManager connectionUIManager;

    private static List<EditorObject> editorObjects;
    private static List<Actor> actors;
    private static List<UIBase> uiElements;

    private static Graph graph;

    private static Dictionary<int, NodeVisual?> nodeToNodeUIDict;
    private static Dictionary<int, PortVisual?> portToPortUIDict;
    private static Dictionary<int, List<NodeVisual>> varToNodeUIsDict;

    private static int? currentlySelectedVarId;

    public static int ScreenWidth { get => screenWidth; }
    public static int ScreenHeight { get => screenHeight; }
    public static Camera2D RCamera { get => camera.RaylibCam2D; }
    public static Graph Graph { get => graph; }
    public static List<EditorObject> EditorObjects { get => editorObjects; }
    public static List<Actor> Actors { get => actors; }
    public static List<UIBase> UIElements { get => uiElements; }
    public static EditorCamera2D Camera { get => camera; }
    public static Canvas Canvas { get => canvas; }
    public static ConnectionVisualManager ConnectionUIManager { get => connectionUIManager; }
    public static Dictionary<int, NodeVisual?> NodeToNodeUIDict { get => nodeToNodeUIDict; }
    public static Dictionary<int, PortVisual?> PortToPortUIDict { get => portToPortUIDict; }
    public static int? CurrentlySelectedVarId { get => currentlySelectedVarId; }

    public static event Action<PointerInteractEventData, EditorObject?> OnAnyPointerDown;
    public static event Action OnHandleInputComplete;

    public static void NotifyAddVar(int varId)
    {
        varToNodeUIsDict.Add(varId, []);
    }


    public static void NotifyAddNode(int nodeId)
    {
        nodeToNodeUIDict.Add(nodeId, null);
    }

    public static void NotifyAddPort(int portId)
    {
        portToPortUIDict.Add(portId, null);
    }

    public static void NotifyConnectNodeAndUI(int nodeId, NodeVisual nui)
    {
        if (nodeToNodeUIDict.ContainsKey(nodeId))
            nodeToNodeUIDict[nodeId] = nui;
    }

    public static void NotifyConnectPortAndUI(int portId, PortVisual pui)
    {
        if (portToPortUIDict.ContainsKey(portId))
            portToPortUIDict[portId] = pui;
    }

    public static void NotifyDisconnectNodeAndUI(int nodeId)
    {
        if (nodeToNodeUIDict.ContainsKey(nodeId))
            nodeToNodeUIDict[nodeId] = null;
    }

    public static void NotifyDisconnectPortAndUI(int portId)
    {
        if (portToPortUIDict.ContainsKey(portId))
            portToPortUIDict[portId] = null;
    }

    public static void NotifyRemoveNode(int nodeId)
    {
        nodeToNodeUIDict.Remove(nodeId);
    }

    public static void NotifyRemovePort(int portId)
    {
        portToPortUIDict.Remove(portId);
    }

    public static void NotifyRemoveVar(int varId)
    {
        varToNodeUIsDict.Remove(varId);
    }

    public static void NotifyRemoveConnection(int sourcePort, int targetPort)
    {
        PortVisual? sourceP = portToPortUIDict[sourcePort];
        PortVisual? targetP = portToPortUIDict[targetPort];

        if (sourceP == null || targetP == null)
        {
            Console.WriteLine("Error: Couldn't get port ui from ports while notify remove connection!");
            return;
        }

        connectionUIManager.OnRemoveConnection(sourceP, targetP);
    }

    private static void RemoveNodeAndUI(NodeVisual nodeVis)
    {
        int nodeId = nodeVis.NodeId;
        graph.RemoveNode(nodeId);

        actors.Remove(nodeVis);
        nodeVis.Delete();
    }

    public static void Start()
    {
        Setup();
        Run();
        Cleanup();
    }

    private static void Setup()
    {
        graph = new Graph();

        screenWidth = 1024;
        screenHeight = 576;

        Raylib.InitWindow(screenWidth, screenHeight, "Raylib Node Library");

        nodeToNodeUIDict = [];
        portToPortUIDict = [];
        varToNodeUIsDict = [];

        actors = [];
        uiElements = [];
        editorObjects = [];

        camera = new(screenWidth, screenHeight);
        editorObjects.Add(camera);

        connectionUIManager = new(null);

        canvas = new Canvas(null, (button, editorObj) =>
        {
            if (button.ButtonText == "Add Node")
            {
                Vector2 mp = InteractionManager.InputContext.mouseWorldPosition;

                Node n = graph.AddNode(0, 0);
                List<(int, string)> inputPortIds = n.InputPortIdNames;
                List<(int, string)> outputPortIds = n.OutputPortIdNames;

                NodeVisual testNodeVis = new(n.Id, inputPortIds, outputPortIds,
                    [(UIElementType.Text, new TextDesc("Enter Text:", Raylib.Fade(Color.White, 0.65f))),
                    (UIElementType.InputField, new InputFieldDesc("", "", 150, 25)),
                    (UIElementType.Selectable, new SelectableDesc("Red", 150, 25, (sel) => { })),
                    (UIElementType.Selectable, new SelectableDesc("Blue", 150, 25, (sel) => { })),
                    (UIElementType.Button, new ButtonDesc("Add Output Port", 150, 25, (b) => Console.WriteLine("CLICKED! " + b)))],
                    "Node", mp.X, mp.Y);

                NodeVisual testNodeVis2 = new(n.Id, inputPortIds, outputPortIds,
                    [(UIElementType.Text, new TextDesc("Test Empty!", Raylib.Fade(Color.White, 0.65f))),
                    (UIElementType.InputField, new InputFieldDesc("Enter!", "", 150, 25)),
                    (UIElementType.Button, new ButtonDesc("PRESS!", 150, 25, (btn) => { Console.WriteLine("CLICKED!"); }))],
                    "Node 2", mp.X, mp.Y);

                actors.Add(testNodeVis2);
            }
            else if (button.ButtonText == "Get Var")
            {
                int varId = (int)button.Payload;
                Variable? v = graph.GetVariable(varId);

                if (v == null)
                {
                    Console.WriteLine($"Error: Couldn't find variable of Id {varId} from the graph!");
                    return;
                }

                Vector2 mp = InteractionManager.InputContext.mouseWorldPosition;

                Node n = graph.AddNode(0, 1);

                List<(int, string)> inputPortIds = n.InputPortIdNames;
                List<(int, string)> outputPortIds = n.OutputPortIdNames;

                NodeVisual varGetNodeVis = new(n.Id, inputPortIds, outputPortIds,
                    [(UIElementType.Text, new TextDesc($"{v.VarName} ({v.VarType.Name}):", Raylib.Fade(Color.White, 0.65f)))],
                    "GetVar", mp.X, mp.Y);

                actors.Add(varGetNodeVis);

                if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? nodeVisList))
                    nodeVisList.Add(varGetNodeVis);
                else varToNodeUIsDict.Add(varId, [varGetNodeVis]);
            }
            else if (button.ButtonText == "Delete")
            {
                if (editorObj != null)
                {
                    Actor ac = (Actor)editorObj;
                    NodeVisual? nui = (NodeVisual)ac;

                    if (nui == null)
                    {
                        Console.WriteLine("Didn't receive a not on ctx menu graph delete!");
                        return;
                    }
                    
                    List<int> keys = [.. varToNodeUIsDict.Keys];
                    (int key, int idx)? marked = null;
                    for (int i = 0; i < keys.Count; i++)
                    {
                        List<NodeVisual> nodeViss = varToNodeUIsDict[keys[i]];

                        for (int j = 0; j < nodeViss.Count; j++)
                        {
                            if (nodeViss[j] == nui)
                            {
                                marked = (keys[i], j);
                                break;
                            }
                        }

                        if (marked.HasValue)
                        {
                            varToNodeUIsDict[marked.Value.key].RemoveAt(marked.Value.idx);
                            break;
                        }
                    }

                    RemoveNodeAndUI(nui);
                }
            }
        }, (varId) =>
        {
            // On Var select.
            currentlySelectedVarId = varId;
        },
        () =>
        {
            // On add var.
            graph.AddVariable("New Var", typeof(int), 0);
        },
        (varId) =>
        {
            // On remove var.

            if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? varNodesVisList))
            {
                for (int i = 0; i < varNodesVisList.Count; i++)
                    RemoveNodeAndUI(varNodesVisList[i]);
            }
            else Console.WriteLine("Error: Couldn't get var of id {varId} while removing from VarToNodeUIsDict!");

            graph.RemoveVariable(varId);
        },
        (varId, newName) =>
        {
            if (graph.RenameVariable(varId, newName))
            {
                Variable? v = graph.GetVariable(varId);

                if (v == null)
                {
                    Console.WriteLine("Error: THIS SHOULD NEVER HAPPEN!");
                    return;
                }

                if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? nodeVisList))
                {
                    for (int i = 0; i < nodeVisList.Count; i++)
                        nodeVisList[i].ChangeUIElement(0, new TextDesc($"{newName} ({v.VarType.Name}):", Raylib.Fade(Color.White, 0.65f)));
                }
                else Console.WriteLine($"Error: Couldn't rename variable of id {varId}!");
            }
            else Console.WriteLine($"Error: Couldn't rename variable of id {varId}!");
        });

        uiElements.Add(canvas);

        actors.Add(new GraphBG(15000, 15000));
        actors.Add(connectionUIManager);
    }

    private static void Run()
    {
        while (!Raylib.WindowShouldClose())
        {
            Update();

            Raylib.BeginDrawing();
            {
                Raylib.ClearBackground(new Color((byte)30, (byte)30, (byte)30));
                Render();
            }
            Raylib.EndDrawing();
        }
    }

    private static void Update()
    {
        camera.Update();

        InputContext inputContext = new()
        {
            isLMBCurrentlyHeld = Raylib.IsMouseButtonDown(MouseButton.Left),
            isRMBCurrentlyHeld = Raylib.IsMouseButtonDown(MouseButton.Right),

            wasLMBPressedOnceThisFrame = Raylib.IsMouseButtonPressed(MouseButton.Left),
            wasRMBPressedOnceThisFrame = Raylib.IsMouseButtonPressed(MouseButton.Right),
            wasLMBReleasedOnceThisFrame = Raylib.IsMouseButtonReleased(MouseButton.Left),
            wasRMBReleasedOnceThisFrame = Raylib.IsMouseButtonReleased(MouseButton.Right),

            mouseScreenPosition = Raylib.GetMousePosition(),
            mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), RCamera),

            mouseWheel = Raylib.GetMouseWheelMove(),

            isCtrlDown = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl),
            isShiftDown = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift)
        };

        int keycode;
        while ((keycode = Raylib.GetKeyPressed()) != 0)
            inputContext.keyboardKeysDown.Add((KeyboardKey)keycode);

        InteractionManager.UpdateInputContext(inputContext);

        InteractionManager.HandleInput();
        OnHandleInputComplete?.Invoke();

        for (int i = 0; i < editorObjects.Count; i++)
            editorObjects[i].Update();

        for (int i = 0; i < actors.Count; i++)
            actors[i].Update();

        for (int i = 0; i < uiElements.Count; i++)
            uiElements[i].Update();
    }

    private static void Render()
    {
        RenderWorld();
        RenderUI();
        RenderEditorObjects();
    }

    private static void RenderWorld()
    {
        Raylib.BeginMode2D(camera.RaylibCam2D);

        for (int i = 0; i < actors.Count; i++)
            actors[i].Render();

        Raylib.EndMode2D();
    }

    private static void RenderUI()
    {
        Raylib.DrawFPS(screenWidth - 150, 10);

        for (int i = 0; i < uiElements.Count; i++)
            uiElements[i].Render();
    }

    private static void RenderEditorObjects()
    {
        for (int i = 0; i < editorObjects.Count; i++)
            editorObjects[i].Render();
    }

    public static void Cleanup()
    {
        Raylib.CloseWindow();
    }

    public static void HandleGlobalPointerEvent(PointerInteractEventData evt, PointerEventType pet)
    {
        if (!evt.IsDragRelated && pet == PointerEventType.Up && (evt.mouseButton == MouseButton.Right || evt.mouseButton == MouseButton.Left))
            canvas.OnMouseUp(evt);

        if (evt.IsDragRelated && pet == PointerEventType.Down && evt.mouseButton == MouseButton.Right)
            camera.OnMouseDown(evt);

        if (!evt.IsDragRelated && (pet == PointerEventType.Up || pet == PointerEventType.Down) && evt.mouseButton == MouseButton.Middle && evt is ScrollEventData sed)
            camera.OnScroll(sed);
    }

    public static void HandleGlobalKBEvents(KeyInteractEventData keyEvent)
    {
        // No object is focused. Handle global Editor Hotkeys here.
        // e.g., Ctrl+S to save the graph, Ctrl+Z to undo.
    }

    public static void NotifyAnyPointerDown(PointerInteractEventData evt, EditorObject? target)
    {
        OnAnyPointerDown?.Invoke(evt, target);
    }
}