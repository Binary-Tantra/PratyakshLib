namespace RaylibNodeLibrary;

using Raylib_cs;
using RaylibNodeLibrary.DataModel;
using RaylibNodeLibrary.UI;
using System.Numerics;

public class NodeVisual : Actor, IPointerInteractable, IDragable
{
    private readonly int nodeId;

    private Rectangle rect;
    private Rectangle headerRect;
    private string title;

    private float bgRectRoundness = 0.1f;
    private float bgRectSegments = 0.0f;
    private float bgRectOutlineThickness = 1.0f;

    private float hdRectRoundness = 0.2f;
    private float hdRectSegments = 0.0f;
    private float hdRectOutlineThickness = 1.0f;

    private Color titleColor = Color.White;

    private Color bgRectFillColor = Raylib.Fade(Color.Black, 0.65f);
    private Color bgRectBorderColor = Raylib.Fade(Color.DarkBlue, 0.4f);

    private Color hdRectFillColor = new Color((byte)125, (byte)50, (byte)50, (byte)255);
    private Color hdRectBorderColor = Raylib.Fade(Color.DarkBlue, 0.4f);

    private bool isDragging;
    private Vector2 dragOffset;

    private List<PortVisual> inputPorts;
    private List<PortVisual> outputPorts;

    private PortVisual? potConnectionStartPortUI;
    private WireVisual? potConnectionWireUI;

    public int NodeId { get => nodeId; }

    private ChildLayout nodeBodyLayout;

    private List<(int id, string name)> inputPortIdNames;
    private List<(int id, string name)> outputPortIdNames;

    private List<(UIElementType elemType, UIElementDescription elemDesc)> bodyUIElements;

    public NodeVisual(int nodeId,
                      List<(UIElementType elemType, UIElementDescription elemDesc)> bodyUIElements,
                      string title, float posX, float posY, Drawable? parent = null) : base(parent)
    {
        this.nodeId = nodeId;

        Engine.NotifyConnectNodeAndUI(nodeId, this);

        selfInteractable = true;

        this.title = title;

        relativePosition.X = posX;
        relativePosition.Y = posY;
        
        this.bodyUIElements = bodyUIElements;

        UpdateNodeVisual();
    }

    public void UpdateTitle(string newTitle)
    {
        title = newTitle;
    }

    public void UpdateNodeVisual()
    {
        Node? n = Engine.Graph.GetNode(nodeId);

        if (n == null)
        {
            Console.WriteLine($"Error: While trying to update node visual, the connected node (id: {nodeId}) was not valid!");
            return;
        }

        inputPortIdNames = n.InputPortIdNames;
        outputPortIdNames = n.OutputPortIdNames;

        float width = 200;
        float height = 75;

        float headerWidth = width;
        float headerHeight = 15;

        int portsInitialYOffset = 30;
        int portsPadding = 15;
        int portsSpacing = 35;
        int portsMax = Math.Max(inputPortIdNames.Count, outputPortIdNames.Count);

        if (portsMax > 2)
        {
            height += (portsMax - 2) * portsSpacing + portsPadding;
        }

        int uiElementsNeededHeight = portsInitialYOffset + (bodyUIElements.Count * 25) + ((bodyUIElements.Count - 1) * 5) + 10; // + 10 is extra space at end.

        if (height < uiElementsNeededHeight)
            height = uiElementsNeededHeight;

        int uiElementsNeededWidth = -1;
        for (int i = 0; i < bodyUIElements.Count; i++)
        {
            if (bodyUIElements[i].elemDesc is RectUIEDescription ruid)
            {
                if (ruid.width == null && ruid is HorizontalGroupDesc gDesc)
                {
                    uiElementsNeededWidth = Math.Max(uiElementsNeededWidth, (150 * gDesc.uiElements.Count) + (gDesc.uiElements.Count - 1) * gDesc.spacing);
                }
                else uiElementsNeededWidth = Math.Max(uiElementsNeededWidth, ruid.width ?? 150);
            }
            else uiElementsNeededWidth = Math.Max(uiElementsNeededWidth, Raylib.MeasureText(bodyUIElements[i].elemDesc.text, 15));
        }

        int maxPortTSize = -1;

        for (int i = 0; i < inputPortIdNames.Count; i++)
            maxPortTSize = Math.Max(maxPortTSize, PortVisual.GetPortTSize(inputPortIdNames[i].name));

        for (int i = 0; i < outputPortIdNames.Count; i++)
            maxPortTSize = Math.Max(maxPortTSize, PortVisual.GetPortTSize(outputPortIdNames[i].name));

        if (maxPortTSize == -1)
            maxPortTSize = 10;

        // The first + 10 is what the port adds while rendering. The second is extra padding. Where there is only one + 10, then in those cases it is just extra padding.
        int bodyHPaddingInputSide = inputPortIdNames.Count > 0 ? maxPortTSize + portsPadding + 10 + 10 : portsPadding + 10;
        int bodyHPaddingOutputSize = outputPortIdNames.Count > 0 ? maxPortTSize + portsPadding + 10 + 10 : portsPadding + 10;

        int totalBodyHPadding = bodyHPaddingInputSide + bodyHPaddingOutputSize;

        if (width - totalBodyHPadding < uiElementsNeededWidth)
        {
            width = uiElementsNeededWidth + totalBodyHPadding;
            headerWidth = width;
        }

        int bodyWidth = (int)(width - totalBodyHPadding);

        inputPorts = [];
        outputPorts = [];

        for (int i = 0; i < inputPortIdNames.Count; i++)
        {
            Port p = n.InputPorts[inputPortIdNames[i].id];
            PortVisual pv = new(inputPortIdNames[i].id, DataModel.PortFlowType.Input, new Vector2(portsPadding, portsInitialYOffset + i * portsSpacing), inputPortIdNames[i].name, p.DataType.Id, this);
            inputPorts.Add(pv);
        }

        for (int i = 0; i < outputPortIdNames.Count; i++)
        {
            Port p = n.OutputPorts[outputPortIdNames[i].id];
            PortVisual pv = new(outputPortIdNames[i].id, DataModel.PortFlowType.Output, new Vector2(width - portsPadding, portsInitialYOffset + i * portsSpacing), outputPortIdNames[i].name, p.DataType.Id, this);
            outputPorts.Add(pv);
        }

        rect = new Rectangle(relativePosition.X, relativePosition.Y, width, height);
        headerRect = new Rectangle(relativePosition.X, relativePosition.Y, headerWidth, headerHeight);

        nodeBodyLayout = new ChildLayout(bodyUIElements,
                                            bodyHPaddingInputSide,
                                            portsInitialYOffset,
                                            bodyWidth,
                                            (int)height, this);
    }

    protected override Drawable? OnChildrenHitTest(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        for (int i = inputPorts.Count - 1; i >= 0; i--)
        {
            var hit = inputPorts[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        for (int i = outputPorts.Count - 1; i >= 0; i--)
        {
            var hit = outputPorts[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        return nodeBodyLayout.HitTest(mouseScreenPosition, mouseWorldPosition);
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return rect;
    }

    protected override void OnUpdate()
    {
        for (int i = 0; i < inputPorts.Count; i++)
            inputPorts[i].Update();

        for (int i = 0; i < outputPorts.Count; i++)
            outputPorts[i].Update();

        nodeBodyLayout.Update();

        potConnectionWireUI?.Update();
    }

    protected override void OnDraw()
    {
        rect.X = Position.X;
        rect.Y = Position.Y;
        headerRect.X = Position.X;
        headerRect.Y = Position.Y;

        Raylib.DrawRectangleRounded(rect, bgRectRoundness, (int)bgRectSegments, bgRectFillColor);
        Raylib.DrawRectangleRoundedLinesEx(rect, bgRectRoundness, (int)bgRectSegments, bgRectOutlineThickness, bgRectBorderColor);
        Raylib.DrawRectangleRounded(headerRect, hdRectRoundness, (int)hdRectSegments, hdRectFillColor);
        Raylib.DrawRectangleRoundedLinesEx(headerRect, hdRectRoundness, (int)hdRectSegments, hdRectOutlineThickness, hdRectBorderColor);

        LayoutEngine.DrawTextAbsolute(title, (int)rect.X + 5, (int)rect.Y, titleColor, 15, Vector2.Zero);

        nodeBodyLayout.Render();

        for (int i = 0; i < inputPorts.Count; i++)
            inputPorts[i].Render();

        for (int i = 0; i < outputPorts.Count; i++)
            outputPorts[i].Render();

        potConnectionWireUI?.Render();
    }

    protected override void OnDelete()
    {
        nodeBodyLayout.Delete();

        for (int i = 0; i < inputPorts.Count; i++)
            inputPorts[i].Delete();

        for (int i = 0; i < outputPorts.Count; i++)
            outputPorts[i].Delete();

        potConnectionWireUI?.Delete();

        Engine.NotifyDisconnectNodeAndUI(nodeId);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        return false;
    }

    public bool OnDragStart(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        InteractionManager.CapturePointer(this);

        isDragging = true;
        dragOffset = new Vector2(evt.WorldPosition.X - RelativePosition.X, evt.WorldPosition.Y - RelativePosition.Y);

        return true;
    }

    public void OnDrag(PointerInteractEventData evt)
    {
        if (isDragging)
        {
            relativePosition.X = evt.WorldPosition.X - dragOffset.X;
            relativePosition.Y = evt.WorldPosition.Y - dragOffset.Y;
        }
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        isDragging = false;

        InteractionManager.ReleasePointer();

        return true;
    }

    private void CleanupWire()
    {
        potConnectionWireUI?.Hide();
        potConnectionWireUI = null;
    }

    public void UIConnectionStart(PortVisual source)
    {
        potConnectionStartPortUI = source;
        potConnectionWireUI = new WireVisual(source);
        potConnectionWireUI.SetColor(source.PortColor);
        potConnectionWireUI.SetThickness(source.IsExecution ? 3.0f : 1.5f);

        potConnectionWireUI.SetStartPos(source.Position);
        potConnectionWireUI.SetEndPos(source.Position);

        potConnectionWireUI.Show();
    }

    public void UIConnectionMove(PointerInteractEventData evt)
    {
        potConnectionWireUI?.SetEndPos(evt.WorldPosition);
    }

    public void UIConnectionSuccess(PortVisual sourceUI, PortVisual targetUI)
    {
        Engine.ConnectionUIManager.OnAddNewConnection(sourceUI, targetUI);
        CleanupWire();
    }

    public void UIConnectionCanceled(PortVisual source)
    {
        if (potConnectionStartPortUI != source)
            Console.WriteLine($"Error: UI Connection started with port {potConnectionStartPortUI} and aborted with {source}!");

        potConnectionStartPortUI = null;
        CleanupWire();
    }

    public bool UIConnectionComplete(PortVisual source, PortVisual connect)
    {
        bool success = Engine.Graph.AddConnection(source.ParentNodeId, source.PortId, connect.ParentNodeId, connect.PortId);

        if (success) UIConnectionSuccess(source, connect);
        else UIConnectionCanceled(source);

        return success;
    }

    public void ChangeUIElement(int elementIdx, UIElementDescription desc)
    {
        nodeBodyLayout.ChangeUIElement(elementIdx, desc);
    }

    public List<object?> GetUIStatePayloads() => nodeBodyLayout.GetUIStatePayloads();
    public void SetUIStatePayloads(List<System.Text.Json.JsonElement?> savedPayloads) => nodeBodyLayout.SetUIStatePayloads(savedPayloads);
}