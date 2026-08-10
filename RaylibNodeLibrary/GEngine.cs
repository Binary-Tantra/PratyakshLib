namespace RaylibNodeLibrary;

using Pratyaksh.Core;
using Pratyaksh.UI;
using Pratyaksh.UI.UIElements;
using RaylibNodeLibrary.DataModel;
using RaylibNodeLibrary.UI;
using System.Numerics;

public class GEngine : Engine
{
    public override float DeltaTime => Raylib_cs.Raylib.GetFrameTime();

    private static EditorCamera2D camera;
    private static Canvas canvas;
    private static int searchMenuIdx;
    private static int contextMenuIdx;

    private static ConnectionVisualManager connectionUIManager;

    private static Graph graph;

    private static Dictionary<int, NodeVisual?> nodeToNodeUIDict;
    private static Dictionary<int, PortVisual?> portToPortUIDict;
    private static Dictionary<int, List<NodeVisual>> varToNodeUIsDict;

    private static int? currentlySelectedObjectId;

    private static Raylib_cs.Font defaultFont;

    public float ScreenWidth { get => InteractionManager.WorldToScreenTransformer.GetWidth(); }
    public float ScreenHeight { get => InteractionManager.WorldToScreenTransformer.GetHeight(); }
    public static Raylib_cs.Camera2D RCamera { get => camera.RaylibCam2D; }
    public static Graph Graph { get => graph; }
    public static EditorCamera2D Camera { get => camera; }
    public static Canvas Canvas { get => canvas; }
    public static ConnectionVisualManager ConnectionUIManager { get => connectionUIManager; }
    public static Dictionary<int, NodeVisual?> NodeToNodeUIDict { get => nodeToNodeUIDict; }
    public static Dictionary<int, PortVisual?> PortToPortUIDict { get => portToPortUIDict; }
    public static int? CurrentlySelectedObjectId { get => currentlySelectedObjectId; }
    public static Dictionary<int, Raylib_cs.Color> DataTypeColors { get; private set; }
    public static NodeRegistry NodeRegistry { get; private set; }
    public static Raylib_cs.Font DefaultFont { get => defaultFont; }

    public static event Action<int?> OnGlobalSelectionChanged;

    private static List<(Rectangle rect, Raylib_cs.Color color)> deferredRects = new();

    public GEngine(int screenWidth, int screenHeight) : base()
    {
        camera = new(screenWidth, screenHeight);
        InteractionManager itm = new(camera);

        Init(itm);

        InteractionManager.GlobalPointerEvent += HandleGlobalPointerEvent;
        InteractionManager.GlobalKBEvent += HandleGlobalKBEvents;
    }

    public void HandleGlobalPointerEvent(PointerInteractEventData evt, PointerEventType pet, bool wasBubble)
    {
        if (pet == PointerEventType.Up && (evt.MouseButton == MouseButton.Right || evt.MouseButton == MouseButton.Left))
            canvas.OnMouseUp(evt);

        if (pet == PointerEventType.DragStart && evt.MouseButton == MouseButton.Right)
            camera.OnDragStart(evt);

        if ((pet == PointerEventType.Up || pet == PointerEventType.Down) && evt.MouseButton == MouseButton.Middle && evt is ScrollEventData sed)
            camera.OnScroll(sed);
    }

    public void HandleGlobalKBEvents(KeyInteractEventData keyEvent)
    {
        // No object is focused. Handle global Editor Hotkeys here.
        // e.g., Ctrl+S to save the graph, Ctrl+Z to undo.
        if (keyEvent.Key == KeyboardKey.S && InteractionManager.InputContext.isCtrlDown)
        {
            string json = DataModel.Serialization.GraphSerializer.Serialize(graph, nodeToNodeUIDict, NodeRegistry, IdGen.CurrentId, canvas);
            File.WriteAllText("save.json", json);
            Console.WriteLine("Saved graph to save.json");
        }
        else if (keyEvent.Key == KeyboardKey.L && InteractionManager.InputContext.isCtrlDown)
        {
            if (File.Exists("save.json"))
            {
                string json = File.ReadAllText("save.json");
                var data = DataModel.Serialization.GraphSerializer.Deserialize(json);
                if (data != null)
                {
                    ReconstructGraph(data);
                    Console.WriteLine("Loaded graph from save.json");
                }
            }
        }
    }

    public static void NotifyAddVar(int varId)
    {
        varToNodeUIsDict.Add(varId, []);
        
        if (graph != null)
        {
            Variable? v = graph.GetVariable(varId);
            if (v != null)
            {
                NodeTemplate t = new($"Get {v.VarName}", "Variables", [], [v.VarType.Name], 
                    [(UIElementType.Text, new TextDesc($"{v.VarName} ({v.VarType.Name}):", Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.65f)))], varId);
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

    private void RemoveNodeAndUI(NodeVisual nodeVis)
    {
        int nodeId = nodeVis.NodeId;
        graph.RemoveNode(nodeId);

        actors.Remove(nodeVis);
        nodeVis.Delete();
    }

    private void SetupDefaultTypeColors()
    {
        DataTypeColors = [];

        DataType? execType = graph.Types.GetType("Execution");
        DataType? intType = graph.Types.GetType("Int");
        DataType? floatType = graph.Types.GetType("Float");
        DataType? numType = graph.Types.GetType("Number");
        DataType? strType = graph.Types.GetType("String");
        DataType? boolType = graph.Types.GetType("Bool");

        if (execType != null)
            DataTypeColors.Add(execType.Id, Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.55f));
        
        if (intType != null)
            DataTypeColors.Add(intType.Id, Raylib_cs.Color.Green);
        
        if (floatType != null)
            DataTypeColors.Add(floatType.Id, Raylib_cs.Color.DarkGreen);
        
        if (numType != null)
            DataTypeColors.Add(numType.Id, Raylib_cs.Color.DarkGreen);
        
        if (strType != null)
            DataTypeColors.Add(strType.Id, Raylib_cs.Color.Pink);
        
        if (boolType != null)
            DataTypeColors.Add(boolType.Id, Raylib_cs.Color.Red);
    }

    private void OnSelectVar(int? varId)
    {
        // On Var select.
        currentlySelectedObjectId = varId;
        OnGlobalSelectionChanged?.Invoke(varId);
    }

    private void OnAddVar()
    {
        // On add var.
        DataType? intT = graph.Types.GetType("Int");
        if (intT != null) graph.AddVariable("New Var", intT, 0);
    }

    private void OnRemoveVar(int varId)
    {
        // On remove var.
        if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? varNodesVisList))
        {
            for (int i = 0; i < varNodesVisList.Count; i++)
                RemoveNodeAndUI(varNodesVisList[i]);
        }
        else Console.WriteLine("Error: Couldn't get var of id {varId} while removing from VarToNodeUIsDict!");

        graph.RemoveVariable(varId);
    }

    private void OnRenameVariable(int varId, string newName)
    {
        // On rename var
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
                [(UIElementType.Text, new TextDesc($"{v.VarName} ({v.VarType.Name}):", Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.65f)))], varId);
            
            NodeRegistry.RegisterNode(t);

            if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? nodeVisList))
            {
                for (int i = 0; i<nodeVisList.Count; i++)
                {
                    nodeVisList[i].ChangeUIElement(0, new TextDesc($"{newName} ({v.VarType.Name}):", Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.65f)));
                    nodeVisList[i].UpdateTitle($"Get {v.VarName}");
                    
                    Node? n = graph.GetNode(nodeVisList[i].NodeId);
                    n?.TemplateId = t.Id;
                }
            }
            else Console.WriteLine($"Error: Couldn't rename variable of id {varId}!");
        }
        else Console.WriteLine($"Error: Couldn't rename variable of id {varId}!");
    }

    private void OnChangeVarType(int varId, DataType newType)
    {
        // On change var type
        graph.ChangeVariableType(varId, newType);

        Variable? v = graph.GetVariable(varId);
        if (v == null) return;

        NodeRegistry.UnregisterNodeByPayload(varId);
        NodeTemplate t = new($"Get {v.VarName}", "Variables", [], [v.VarType.Name],
            [(UIElementType.Text, new TextDesc($"{v.VarName} ({v.VarType.Name}):", Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.65f)))], varId);
        NodeRegistry.RegisterNode(t);

        if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? nodeVisList))
        {
            for (int i = 0; i < nodeVisList.Count; i++)
            {
                NodeVisual nui = nodeVisList[i];
                nui.ChangeUIElement(0, new TextDesc($"{v.VarName} ({v.VarType.Name}):", Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.65f)));

                Node? n = graph.GetNode(nui.NodeId);
                if (n != null)
                {
                    n.TemplateId = t.Id;
                    if (n.OutputPorts.Count > 0)
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
        }
    }

    private void OnChangeVarValue(int varId, object newValue)
    {
        Variable? v = graph.GetVariable(varId);
        v?.VarValue = newValue;
    }

    private bool OpenSearchMenu(int x, int y, List<(string, object)> items, Action<object> onItemSelected)
    {
        return canvas.OpenTransPanel<SearchMenu>(searchMenuIdx, x, y, 200, 300, items, onItemSelected, canvas, null) != null;
    }

    private bool OpenContextMenu(int x, int y, EditorObject? potentialTarget)
    {
        List<(string name, object payload)> menuItems = [("Delete", 0)];
        Action<Button> onButtonPressed = (button) => OnCanvasCtxMenuItemSelected(button, potentialTarget);

        return canvas.OpenTransPanel<ContextMenu>(contextMenuIdx, x, y, menuItems, onButtonPressed, canvas, null) != null;
    }

    private void OnCanvasSearchMenuItemSelected(object payload)
    {
        if (payload is not NodeTemplate template)
            return;
        
        Vector2 mp = InteractionManager.InputContext.mouseWorldPosition;
        NodeVisual? visual = NodeRegistry.SpawnNode(graph, template.Id, mp);

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

        canvas.CloseTransPanel(searchMenuIdx);
    }

    private void OnCanvasCtxMenuItemSelected(Button button, EditorObject? editorObj)
    {
        if (button.Payload is int intP && intP != 0) // 0 = Delete
            return;

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

        canvas.CloseTransPanel(contextMenuIdx);
    }

    private bool OnCanvasContextClick(PointerInteractEventData evt, EditorObject? target)
    {
        int posX = (int)evt.ScreenPosition.X;
        int posY = (int)evt.ScreenPosition.Y;

        bool success = false;

        if (Instance.InteractionManager.CurrentlyHit == null)
        {
            List<(string, object)> mis = [];

            foreach (var template in NodeRegistry.AllTemplates)
            {
                mis.Add((template.Name, template));
            }

            success = OpenSearchMenu(posX, posY, mis, OnCanvasSearchMenuItemSelected);
        }
        else if (Instance.InteractionManager.CurrentlyHit is NodeVisual nui)
        {
            success = OpenContextMenu(posX, posY, nui);
        }

        return success;
    }

    protected override void Setup()
    {
        Raylib_cs.Raylib.SetConfigFlags(Raylib_cs.ConfigFlags.ResizableWindow);
        Raylib_cs.Raylib.InitWindow((int)ScreenWidth, (int)ScreenHeight, "Raylib Node Library");

        defaultFont = Raylib_cs.Raylib.LoadFont("../../../Thirdparty/Fonts/Satoshi_Complete/Fonts/TTF/Satoshi-Variable.ttf");
        LayoutEngine.InitSLEDefaultFont(defaultFont);

        nodeToNodeUIDict = [];
        portToPortUIDict = [];
        varToNodeUIsDict = [];

        graph = new Graph();
        
        graph.Types.RegisterDefaultTypes();
        SetupDefaultTypeColors();

        camera.Setup();
        editorObjects.Add(camera);

        connectionUIManager = new(null);

        NodeRegistry = new NodeRegistry();

        NodeRegistry.RegisterNode(new NodeTemplate("Empty Node", "Basic", [], [], [
            (UIElementType.Text, new TextDesc("Test Empty!", Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.65f))),
            (UIElementType.InputField, new InputFieldDesc("Enter!", "", 150, 25)),
            (UIElementType.Button, new ButtonDesc("PRESS!", 150, 25, (btn) => { Console.WriteLine("CLICKED!"); })),
            (UIElementType.Toggle, new ToggleDesc("Toggle", true, 38, 20, (toggle) => { Console.WriteLine("Toggled: " + toggle.IsOn); }))
        ]));

        NodeRegistry.RegisterNode(new NodeTemplate("Class Node", "Basic", 
            ["Execution", "String", "Int", "Number"], 
            ["Execution", "Int", "String"], [
            (UIElementType.Text, new TextDesc("Enter Text:", Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.65f))),
            (UIElementType.InputField, new InputFieldDesc("", "", 150, 25)),
            (UIElementType.Selectable, new SelectableDesc("Red", false, 150, 25, (sel) => { })),
            (UIElementType.Selectable, new SelectableDesc("Blue", false, 150, 25, (sel) => { })),
            (UIElementType.Group, new HorizontalGroupDesc("", 50,
                [(UIElementType.Text, new TextDesc("Enter: ", Raylib_cs.Color.Red)),
                (UIElementType.InputField, new InputFieldDesc("edit...", "", null, null))], 150, 25)),
            (UIElementType.Button, new ButtonDesc("TTEESSTT", 150, 25, (b) => Console.WriteLine("CLICKED! " + b)))
        ]));

        NodeRegistry.RegisterNode(new NodeTemplate("Math Add", "Math",
            ["Float", "Float"], ["Float"], [
            (UIElementType.Text, new TextDesc("A + B", Raylib_cs.Color.White))
        ]));

        canvas = new Canvas((int)ScreenWidth, (int)ScreenHeight, OnCanvasContextClick);
        searchMenuIdx = canvas.AddPanel(null, false, true);
        contextMenuIdx = canvas.AddPanel(null, false, true);

        VariablePanel varPan = new(10, 20, OnSelectVar, OnAddVar, OnRemoveVar, OnRenameVariable, OnChangeVarType, (x, y, items, payload) => OpenSearchMenu(x, y, items, payload), canvas);
        canvas.AddPanel(varPan, false, false);

        InspectorPanel inPan = new(-200 - 10, 20, OnRenameVariable, OnChangeVarType, OnChangeVarValue, (x, y, items, payload) => OpenSearchMenu(x, y, items, payload), canvas, ParentBasis.TopRight);
        canvas.AddPanel(inPan, false, false);

        DemoPanel demoPanel = new(60, 70, canvas);
        canvas.AddPanel(demoPanel, true, false);

        uiElements.Add(canvas);

        actors.Add(new GraphBG(15000, 15000));
        actors.Add(connectionUIManager);
    }

    protected override bool IsCloseRequested()
    {
        return Raylib_cs.Raylib.WindowShouldClose();
    }

    protected override void UpdateScreen()
    {
        int sw = Raylib_cs.Raylib.GetScreenWidth();
        int sh = Raylib_cs.Raylib.GetScreenHeight();

        if (ScreenWidth != sw || ScreenHeight != sh)
        {
            InteractionManager.WorldToScreenTransformer.SetScreenSize(new Vector2(sw, sh));
            canvas.Size = new Vector2(sw, sh);
        }
    }

    protected override InputContext Input()
    {
        InputContext inputContext = new()
        {
            isLMBCurrentlyHeld = Raylib_cs.Raylib.IsMouseButtonDown(Raylib_cs.MouseButton.Left),
            isRMBCurrentlyHeld = Raylib_cs.Raylib.IsMouseButtonDown(Raylib_cs.MouseButton.Right),

            wasLMBPressedOnceThisFrame = Raylib_cs.Raylib.IsMouseButtonPressed(Raylib_cs.MouseButton.Left),
            wasRMBPressedOnceThisFrame = Raylib_cs.Raylib.IsMouseButtonPressed(Raylib_cs.MouseButton.Right),
            wasLMBReleasedOnceThisFrame = Raylib_cs.Raylib.IsMouseButtonReleased(Raylib_cs.MouseButton.Left),
            wasRMBReleasedOnceThisFrame = Raylib_cs.Raylib.IsMouseButtonReleased(Raylib_cs.MouseButton.Right),

            mouseScreenPosition = Raylib_cs.Raylib.GetMousePosition(),
            mouseWorldPosition = Raylib_cs.Raylib.GetScreenToWorld2D(Raylib_cs.Raylib.GetMousePosition(), RCamera),

            mouseWheel = Raylib_cs.Raylib.GetMouseWheelMove(),

            isCtrlDown = Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftControl) || Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.RightControl),
            isShiftDown = Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftShift) || Raylib_cs.Raylib.IsKeyDown(Raylib_cs.KeyboardKey.RightShift)
        };

        int keycode;
        while ((keycode = Raylib_cs.Raylib.GetKeyPressed()) != 0)
        {
            Raylib_cs.KeyboardKey rkey = (Raylib_cs.KeyboardKey)keycode;

            KeyboardKey key;

            if (rkey == Raylib_cs.KeyboardKey.Backspace)
                key = KeyboardKey.Backspace;
            else if (rkey == Raylib_cs.KeyboardKey.Minus)
                key = KeyboardKey.Minus;
            else if (rkey == Raylib_cs.KeyboardKey.Comma)
                key = KeyboardKey.Comma;
            else if (rkey == Raylib_cs.KeyboardKey.Escape)
                key = KeyboardKey.Escape;
            else if (rkey == Raylib_cs.KeyboardKey.Space)
                key = KeyboardKey.Space;
            else if (rkey == Raylib_cs.KeyboardKey.Enter)
                key = KeyboardKey.Enter;
            else if (rkey == Raylib_cs.KeyboardKey.Tab)
                key = KeyboardKey.Tab;
            else if (rkey == Raylib_cs.KeyboardKey.CapsLock)
                key = KeyboardKey.CapsLock;
            else if (rkey == Raylib_cs.KeyboardKey.Left)
                key = KeyboardKey.LeftArrow;
            else if (rkey == Raylib_cs.KeyboardKey.Right)
                key = KeyboardKey.RightArrow;
            else if (rkey == Raylib_cs.KeyboardKey.Up)
                key = KeyboardKey.UpArrow;
            else if (rkey == Raylib_cs.KeyboardKey.Down)
                key = KeyboardKey.DownArrow;
            else
                key = (KeyboardKey)keycode;

            inputContext.keyboardKeysDown.Add(key);
        }

        return inputContext;
    }

    protected override void Render()
    {
        Raylib_cs.Raylib.BeginDrawing();
        {
            Raylib_cs.Raylib.ClearBackground(new Raylib_cs.Color((byte)30, (byte)30, (byte)30));

            RenderWorld();
            RenderUI();
            RenderEditorObjects();
            RenderDeferred();
        }
        Raylib_cs.Raylib.EndDrawing();
    }

    private void RenderWorld()
    {
        Raylib_cs.Raylib.BeginMode2D(camera.RaylibCam2D);

        for (int i = 0; i < actors.Count; i++)
            actors[i].Render();

        Raylib_cs.Raylib.EndMode2D();
    }

    private void RenderUI()
    {
        Raylib_cs.Raylib.DrawFPS((int)InteractionManager.WorldToScreenTransformer.GetWidth() - 150, 10);

        for (int i = 0; i < uiElements.Count; i++)
            uiElements[i].Render();
    }

    private void RenderEditorObjects()
    {
        for (int i = 0; i < editorObjects.Count; i++)
            editorObjects[i].Render();
    }

    private void RenderDeferred()
    {
        for (int i = 0; i < deferredRects.Count; i++)
            Raylib_cs.Raylib.DrawRectangle(
                                (int)deferredRects[i].rect.X,
                                (int)deferredRects[i].rect.Y,
                                (int)deferredRects[i].rect.Width,
                                (int)deferredRects[i].rect.Height,
                                deferredRects[i].color
                                );

        // Clear after draw to wait for next frame.
        deferredRects.Clear();
    }

    public void DrawDeferredWorldSpaceRect(Rectangle rect, Raylib_cs.Color color)
    {
        deferredRects.Add((rect, color));
    }

    protected override void Cleanup()
    {
        Raylib_cs.Raylib.UnloadFont(defaultFont);
        Raylib_cs.Raylib.CloseWindow();
    }

    public void ClearWorkspace()
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

    public void ReconstructGraph(DataModel.Serialization.GraphSaveData data)
    {
        ClearWorkspace();

        IdGen.SetCurrentId(data.MaxId);

        if (data.NodeTemplates != null && data.NodeTemplates.Count > 0)
        {
            NodeRegistry.Clear();
            foreach (var tData in data.NodeTemplates)
            {
                var uiElements = new List<(UIElementType, UIElementDescription)>();
                foreach (var uiData in tData.UIElements)
                {
                    uiElements.Add(DataModel.Serialization.GraphSerializer.DeserializeUIElement(uiData));
                }

                object? payload = null;
                if (tData.Payload.HasValue)
                {
                    if (tData.Payload.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                        payload = tData.Payload.Value.GetInt32();
                    else if (tData.Payload.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                        payload = tData.Payload.Value.GetString();
                }

                NodeTemplate template = new(tData.Id, tData.Name, tData.Category, tData.InputPortTypeNames, tData.OutputPortTypeNames, uiElements, payload);
                NodeRegistry.RegisterNode(template);
            }
        }

        foreach (var vData in data.Variables)
        {
            DataType? varType = graph.Types.GetType(vData.TypeName);
            if (varType == null) continue;

            object value = null;
            if (varType.CSharpType == typeof(int)) value = vData.Value.GetInt32();
            else if (varType.CSharpType == typeof(float)) value = vData.Value.GetSingle();
            else if (varType.CSharpType == typeof(string)) value = vData.Value.GetString();
            else if (varType.CSharpType == typeof(bool)) value = vData.Value.GetBoolean();
            
            Variable v = new(vData.Id, vData.Name, varType, value ?? 0);
            graph.AddVariableExplicit(v);

            if (!varToNodeUIsDict.ContainsKey(v.Id))
                varToNodeUIsDict.Add(v.Id, []);
        }

        foreach (var nd in data.Nodes)
        {
            NodeTemplate? template = NodeRegistry.GetTemplate(nd.TemplateId);

            // Fallback: If exact template ID was not found (e.g. from an older file or unlinked variable template),
            // search for a matching template by port signatures/payload.
            if (template == null)
            {
                foreach (var t in NodeRegistry.AllTemplates)
                {
                    if (t.InputPortTypeNames.Count == nd.InputPorts.Count &&
                        t.OutputPortTypeNames.Count == nd.OutputPorts.Count)
                    {
                        template = t;
                        nd.TemplateId = t.Id;
                        break;
                    }
                }
            }

            Node n = new(nd.Id, nd.TemplateId);

            foreach (var pData in nd.InputPorts)
            {
                DataType? t = graph.Types.GetType(pData.DataTypeName);
                if (t != null)
                {
                    Port p = new(pData.Id, pData.Name, PortFlowType.Input, t);
                    n.AddInputPortExplicit(p);
                }
            }

            foreach (var pData in nd.OutputPorts)
            {
                DataType? t = graph.Types.GetType(pData.DataTypeName);
                if (t != null)
                {
                    Port p = new(pData.Id, pData.Name, PortFlowType.Output, t);
                    n.AddOutputPortExplicit(p);
                }
            }

            graph.AddNodeExplicit(n);

            List<(UIElementType, UIElementDescription)> uiElems = template != null ? template.UIElements : [];
            string titleName = template != null ? template.Name : "Node " + n.Id;

            NodeVisual nodeVis = new(n.Id, uiElems, titleName, nd.PositionX, nd.PositionY);
            if (nd.UIElementValues != null && nd.UIElementValues.Count > 0)
            {
                nodeVis.SetUIStatePayloads(nd.UIElementValues);
            }
            actors.Add(nodeVis);

            if (template != null && template.Payload is int varId)
            {
                if (varToNodeUIsDict.TryGetValue(varId, out List<NodeVisual>? list))
                    list.Add(nodeVis);
                else varToNodeUIsDict.Add(varId, [nodeVis]);
            }
        }

        foreach (var cData in data.Connections)
        {
            Connection c = new(cData.Id, cData.SourcePortId, cData.TargetPortId);
            graph.AddConnectionExplicit(c);

            bool hasSource = portToPortUIDict.TryGetValue(cData.SourcePortId, out PortVisual? sourcePUI);
            bool hasTarget = portToPortUIDict.TryGetValue(cData.TargetPortId, out PortVisual? targetPUI);

            if (hasSource && hasTarget && sourcePUI != null && targetPUI != null)
            {
                connectionUIManager.OnAddNewConnection(sourcePUI, targetPUI);
            }
            else
            {
                Console.WriteLine($"Warning: Could not connect ports visually ({cData.SourcePortId} -> {cData.TargetPortId}) because one or both PortVisuals were not found.");
            }
        }

        if (data.Panels != null && canvas != null)
        {
            canvas.RestorePanelsSaveData(data.Panels);
        }
    }
}