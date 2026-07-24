namespace RaylibNodeLibrary;

using System.Numerics;
using Raylib_cs;
using RaylibNodeLibrary.DataModel;
using RaylibNodeLibrary.UI;

public class Engine
{
    private static int screenWidth;
    private static int screenHeight;

    public static float DeltaTime => Raylib.GetFrameTime();

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

    private static int? currentlySelectedObjectId;

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
    public static int? CurrentlySelectedObjectId { get => currentlySelectedObjectId; }
    public static Dictionary<int, Color> DataTypeColors { get; private set; }
    public static NodeRegistry NodeRegistry { get; private set; }

    public static event Action<PointerInteractEventData, EditorObject?> OnAnyPointerDown;
    public static event Action OnHandleInputComplete;
    public static event Action<int?> OnGlobalSelectionChanged;

    public static void NotifyAddVar(int varId)
    {
        varToNodeUIsDict.Add(varId, []);
        
        if (graph != null)
        {
            Variable? v = graph.GetVariable(varId);
            if (v != null)
            {
                NodeTemplate t = new($"Get {v.VarName}", "Variables", [], [v.VarType.Name], 
                    [(UIElementType.Text, new TextDesc($"{v.VarName} ({v.VarType.Name}):", Raylib.Fade(Color.White, 0.65f)))], varId);
                NodeRegistry.RegisterNode(t);
            }
        }
    }

    public static void NotifyAddNode(int nodeId)
    {
        nodeToNodeUIDict.Add(nodeId, null);
    }

    public static void NotifyUpdateNode(int nodeId)
    {
        if (nodeToNodeUIDict.TryGetValue(nodeId, out NodeVisual? nodeVis))
        {
            if (nodeVis != null)
                nodeVis.UpdateNodeVisual();
            else Console.WriteLine($"Error: While notify update node, connected node vis to Node id {nodeId} was not valid!");
        }
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
        NodeRegistry?.UnregisterNodeByPayload(varId);
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
        graph.Types.RegisterDefaultTypes();

        DataTypeColors = new Dictionary<int, Color>();
        DataType? execType = graph.Types.GetType("Execution");
        if (execType != null) DataTypeColors.Add(execType.Id, Raylib.Fade(Color.White, 0.55f));
        DataType? intType = graph.Types.GetType("Int");
        if (intType != null) DataTypeColors.Add(intType.Id, Color.Green);
        DataType? floatType = graph.Types.GetType("Float");
        if (floatType != null) DataTypeColors.Add(floatType.Id, Color.DarkGreen);
        DataType? numType = graph.Types.GetType("Number");
        if (numType != null) DataTypeColors.Add(numType.Id, Color.DarkGreen);
        DataType? strType = graph.Types.GetType("String");
        if (strType != null) DataTypeColors.Add(strType.Id, Color.Pink);
        DataType? boolType = graph.Types.GetType("Bool");
        if (boolType != null) DataTypeColors.Add(boolType.Id, Color.Red);

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

        NodeRegistry = new NodeRegistry();

        NodeRegistry.RegisterNode(new NodeTemplate("Empty Node", "Basic", [], [], [
            (UIElementType.Text, new TextDesc("Test Empty!", Raylib.Fade(Color.White, 0.65f))),
            (UIElementType.InputField, new InputFieldDesc("Enter!", "", 150, 25)),
            (UIElementType.Button, new ButtonDesc("PRESS!", 150, 25, (btn) => { Console.WriteLine("CLICKED!"); })),
            (UIElementType.Toggle, new ToggleDesc("Toggle", true, 38, 20, (toggle) => { Console.WriteLine("Toggled: " + toggle.IsOn); }))
        ]));

        NodeRegistry.RegisterNode(new NodeTemplate("Class Node", "Basic", 
            ["Execution", "String", "Int", "Number"], 
            ["Execution", "Int", "String"], [
            (UIElementType.Text, new TextDesc("Enter Text:", Raylib.Fade(Color.White, 0.65f))),
            (UIElementType.InputField, new InputFieldDesc("", "", 150, 25)),
            (UIElementType.Selectable, new SelectableDesc("Red", 150, 25, (sel) => { })),
            (UIElementType.Selectable, new SelectableDesc("Blue", 150, 25, (sel) => { })),
            (UIElementType.Group, new HorizontalGroupDesc("", 50,
                [(UIElementType.Text, new TextDesc("Enter: ", Color.Red)),
                (UIElementType.InputField, new InputFieldDesc("edit...", "", null, null))], 150, 25)),
            (UIElementType.Button, new ButtonDesc("TTEESSTT", 150, 25, (b) => Console.WriteLine("CLICKED! " + b)))
        ]));

        NodeRegistry.RegisterNode(new NodeTemplate("Math Add", "Math",
            ["Float", "Float"], ["Float"], [
            (UIElementType.Text, new TextDesc("A + B", Color.White))
        ]));

        NodeRegistry = new NodeRegistry();

        NodeRegistry.RegisterNode(new NodeTemplate("Empty Node", "Basic", [], [], [
            (UIElementType.Text, new TextDesc("Test Empty!", Raylib.Fade(Color.White, 0.65f))),
            (UIElementType.InputField, new InputFieldDesc("Enter!", "", 150, 25)),
            (UIElementType.Button, new ButtonDesc("PRESS!", 150, 25, (btn) => { Console.WriteLine("CLICKED!"); })),
            (UIElementType.Toggle, new ToggleDesc("Toggle", true, 38, 20, (toggle) => { Console.WriteLine("Toggled: " + toggle.IsOn); }))
        ]));

        NodeRegistry.RegisterNode(new NodeTemplate("Class Node", "Basic", 
            ["Execution", "String", "Int", "Number"], 
            ["Execution", "Int", "String"], [
            (UIElementType.Text, new TextDesc("Enter Text:", Raylib.Fade(Color.White, 0.65f))),
            (UIElementType.InputField, new InputFieldDesc("", "", 150, 25)),
            (UIElementType.Selectable, new SelectableDesc("Red", 150, 25, (sel) => { })),
            (UIElementType.Selectable, new SelectableDesc("Blue", 150, 25, (sel) => { })),
            (UIElementType.Group, new HorizontalGroupDesc("", 50,
                [(UIElementType.Text, new TextDesc("Enter: ", Color.Red)),
                (UIElementType.InputField, new InputFieldDesc("edit...", "", null, null))], 150, 25)),
            (UIElementType.Button, new ButtonDesc("TTEESSTT", 150, 25, (b) => Console.WriteLine("CLICKED! " + b)))
        ]));

        NodeRegistry.RegisterNode(new NodeTemplate("Math Add", "Math",
            ["Float", "Float"], ["Float"], [
            (UIElementType.Text, new TextDesc("A + B", Color.White))
        ]));

        canvas = new Canvas(null, (payload, editorObj) =>
        {
            if (payload is NodeTemplate template)
            {
                Vector2 mp = InteractionManager.InputContext.mouseWorldPosition;
                NodeVisual? visual = NodeRegistry.SpawnNode(graph, template.Name, mp);
                if (visual != null)
                {
                    actors.Add(visual);
                    if (template.Payload is int varId)
                    {
                        if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? list))
                            list.Add(visual);
                        else varToNodeUIsDict.Add(varId, [visual]);
                    }
                }
            }
            else if (payload is string payloadStr && payloadStr == "Delete")
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
            currentlySelectedObjectId = varId;
            OnGlobalSelectionChanged?.Invoke(varId);
        },
        () =>
        {
            // On add var.
            DataType? intT = graph.Types.GetType("Int");
            if (intT != null) graph.AddVariable("New Var", intT, 0);
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

                NodeRegistry.UnregisterNodeByPayload(varId);
                NodeTemplate t = new($"Get {v.VarName}", "Variables", [], [v.VarType.Name], 
                    [(UIElementType.Text, new TextDesc($"{v.VarName} ({v.VarType.Name}):", Raylib.Fade(Color.White, 0.65f)))], varId);
                NodeRegistry.RegisterNode(t);

                if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? nodeVisList))
                {
                    for (int i = 0; i < nodeVisList.Count; i++)
                    {
                        nodeVisList[i].ChangeUIElement(0, new TextDesc($"{newName} ({v.VarType.Name}):", Raylib.Fade(Color.White, 0.65f)));
                        nodeVisList[i].UpdateTitle($"Get {v.VarName}");
                    }
                }
                else Console.WriteLine($"Error: Couldn't rename variable of id {varId}!");
            }
            else Console.WriteLine($"Error: Couldn't rename variable of id {varId}!");
        },
        (varId, newType) =>
        {
            graph.ChangeVariableType(varId, newType);

            Variable? v = graph.GetVariable(varId);
            if (v == null) return;

            NodeRegistry.UnregisterNodeByPayload(varId);
            NodeTemplate t = new($"Get {v.VarName}", "Variables", [], [v.VarType.Name], 
                [(UIElementType.Text, new TextDesc($"{v.VarName} ({v.VarType.Name}):", Raylib.Fade(Color.White, 0.65f)))], varId);
            NodeRegistry.RegisterNode(t);

            if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? nodeVisList))
            {
                for (int i = 0; i < nodeVisList.Count; i++)
                {
                    NodeVisual nui = nodeVisList[i];
                    nui.ChangeUIElement(0, new TextDesc($"{v.VarName} ({v.VarType.Name}):", Raylib.Fade(Color.White, 0.65f)));

                    Node? n = graph.GetNode(nui.NodeId);
                    if (n != null && n.OutputPorts.Count > 0)
                    {
                        int portId = n.OutputPortIds[0];
                        Port p = n.OutputPorts[portId];
                        
                        p.DataType = newType;
                        p.PortName = newType.Name;

                        if (portToPortUIDict.TryGetValue(portId, out PortVisual? pui) && pui != null)
                            pui.UpdateDataType(newType.Id, newType.Name);

                        graph.DisconnectIncompatibleConnections(portId);
                    }
                }
            }
        },
        (varId, newValue) =>
        {
            Variable? v = graph.GetVariable(varId);
            if (v != null)
            {
                v.VarValue = newValue;
            }
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
        RenderDeferred();
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

    private static void RenderDeferred()
    {
        for (int i = 0; i < deferredRects.Count; i++)
            Raylib.DrawRectangle(
                                (int)deferredRects[i].rect.X,
                                (int)deferredRects[i].rect.Y,
                                (int)deferredRects[i].rect.Width,
                                (int)deferredRects[i].rect.Height,
                                deferredRects[i].color
                                );

        // Clear after draw to wait for next frame.
        deferredRects.Clear();
    }

    private static List<(Rectangle rect, Color color)> deferredRects = new();

    public static void DrawDeferredWorldSpaceRect(Rectangle rect, Color color)
    {
        deferredRects.Add((rect, color));
    }

    public static void Cleanup()
    {
        Raylib.CloseWindow();
    }

    public static void ClearWorkspace()
    {
        graph.Clear();

        currentlySelectedObjectId = null;

        // Collect node visual actors to delete
        List<NodeVisual> nodesToDelete = new();
        foreach (var nui in nodeToNodeUIDict.Values)
        {
            if (nui != null) nodesToDelete.Add(nui);
        }

        foreach (var nui in nodesToDelete)
        {
            actors.Remove(nui);
            nui.Delete();
        }

        nodeToNodeUIDict.Clear();
        portToPortUIDict.Clear();
        varToNodeUIsDict.Clear();
    }

    public static void ReconstructGraph(RaylibNodeLibrary.DataModel.Serialization.GraphSaveData data)
    {
        ClearWorkspace();

        RaylibNodeLibrary.DataModel.IdGen.SetCurrentId(data.MaxId);

        foreach (var vData in data.Variables)
        {
            DataType? varType = graph.Types.GetType(vData.TypeName);
            if (varType == null) continue;

            object value = null;
            if (varType.CSharpType == typeof(int)) value = vData.Value.GetInt32();
            else if (varType.CSharpType == typeof(float)) value = vData.Value.GetSingle();
            else if (varType.CSharpType == typeof(string)) value = vData.Value.GetString();
            else if (varType.CSharpType == typeof(bool)) value = vData.Value.GetBoolean();
            
            Variable v = new Variable(vData.Id, vData.Name, varType, value ?? 0);
            graph.AddVariableExplicit(v);
        }

        foreach (var nd in data.Nodes)
        {
            NodeTemplate? template = NodeRegistry.GetTemplate(nd.TemplateName);
            if (template == null) continue;

            Node n = new Node(nd.Id, nd.TemplateName);

            foreach (var pData in nd.InputPorts)
            {
                DataType? t = graph.Types.GetType(pData.DataTypeName);
                if (t != null)
                {
                    Port p = new Port(pData.Id, pData.Name, PortFlowType.Input, t);
                    n.AddInputPortExplicit(p);
                }
            }

            foreach (var pData in nd.OutputPorts)
            {
                DataType? t = graph.Types.GetType(pData.DataTypeName);
                if (t != null)
                {
                    Port p = new Port(pData.Id, pData.Name, PortFlowType.Output, t);
                    n.AddOutputPortExplicit(p);
                }
            }

            graph.AddNodeExplicit(n);

            NodeVisual nodeVis = new NodeVisual(n.Id, template.UIElements, template.Name, nd.PositionX, nd.PositionY);
            actors.Add(nodeVis);

            if (template.Payload is int varId)
            {
                if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? list))
                    list.Add(nodeVis);
                else varToNodeUIsDict.Add(varId, [nodeVis]);
            }
        }

        foreach (var cData in data.Connections)
        {
            Connection c = new Connection(cData.Id, cData.SourcePortId, cData.TargetPortId);
            graph.AddConnectionExplicit(c);

            PortVisual? sourcePUI = portToPortUIDict[cData.SourcePortId];
            PortVisual? targetPUI = portToPortUIDict[cData.TargetPortId];

            if (sourcePUI != null && targetPUI != null)
            {
                connectionUIManager.OnAddNewConnection(sourcePUI, targetPUI);
            }
        }
    }

    public static void HandleGlobalPointerEvent(PointerInteractEventData evt, PointerEventType pet)
    {
        if (pet == PointerEventType.Up && (evt.mouseButton == MouseButton.Right || evt.mouseButton == MouseButton.Left))
            canvas.OnMouseUp(evt);

        if (pet == PointerEventType.DragStart && evt.mouseButton == MouseButton.Right)
            camera.OnDragStart(evt);

        if ((pet == PointerEventType.Up || pet == PointerEventType.Down) && evt.mouseButton == MouseButton.Middle && evt is ScrollEventData sed)
            camera.OnScroll(sed);
    }

    public static void HandleGlobalKBEvents(KeyInteractEventData keyEvent)
    {
        // No object is focused. Handle global Editor Hotkeys here.
        // e.g., Ctrl+S to save the graph, Ctrl+Z to undo.
        if (keyEvent.Key == KeyboardKey.S && InteractionManager.InputContext.isCtrlDown)
        {
            string json = RaylibNodeLibrary.DataModel.Serialization.GraphSerializer.Serialize(graph, nodeToNodeUIDict, RaylibNodeLibrary.DataModel.IdGen.CurrentId);
            System.IO.File.WriteAllText("save.json", json);
            Console.WriteLine("Saved graph to save.json");
        }
        else if (keyEvent.Key == KeyboardKey.L && InteractionManager.InputContext.isCtrlDown)
        {
            if (System.IO.File.Exists("save.json"))
            {
                string json = System.IO.File.ReadAllText("save.json");
                var data = RaylibNodeLibrary.DataModel.Serialization.GraphSerializer.Deserialize(json);
                if (data != null)
                {
                    ReconstructGraph(data);
                    Console.WriteLine("Loaded graph from save.json");
                }
            }
        }
    }

    public static void NotifyAnyPointerDown(PointerInteractEventData evt, EditorObject? target)
    {
        OnAnyPointerDown?.Invoke(evt, target);
    }
}