namespace RaylibNodeLibrary;

using System.Numerics;
using Raylib_cs;

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

    public NodeVisual(int nodeId, List<int> inputPortIds, List<int> outputPortIds, string title, float posX, float posY, Drawable? parent = null) : base(parent)
    {
        this.nodeId = nodeId;

        Engine.NotifyConnectNodeAndUI(nodeId, this);

        selfInteractable = true;

        this.title = title;

        relativePosition.X = posX;
        relativePosition.Y = posY;
        
        float w = 200;
        float h = 75;

        float hw = w;
        float hh = 15;

        int pInitialYOffset = 30;
        int pPadding = 15;
        int pSpacing = 35;
        int portsMax = Math.Max(inputPortIds.Count, outputPortIds.Count);
        
        if (portsMax > 2)
        {
            h += (portsMax - 2) * pSpacing + pPadding;
        }

        inputPorts = [];
        outputPorts = [];

        for (int i = 0; i < inputPortIds.Count; i++)
            inputPorts.Add(new PortVisual(inputPortIds[i], DataModel.PortFlowType.Input, new Vector2(pPadding, pInitialYOffset + i * pSpacing), $"InPort {inputPortIds[i]}", this));

        for (int i = 0; i < outputPortIds.Count; i++)
            outputPorts.Add(new PortVisual(outputPortIds[i], DataModel.PortFlowType.Output, new Vector2(w - pPadding, pInitialYOffset + i * pSpacing), $"OutPort {outputPortIds[i]}", this));

        rect = new Rectangle(relativePosition.X, relativePosition.Y, w, h);
        headerRect = new Rectangle(relativePosition.X, relativePosition.Y, hw, hh);
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

        return null;
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

        Raylib.DrawText(title, (int)rect.X + 5, (int)rect.Y, 15, titleColor);

        for (int i = 0; i < inputPorts.Count; i++)
            inputPorts[i].Render();

        for (int i = 0; i < outputPorts.Count; i++)
            outputPorts[i].Render();

        potConnectionWireUI?.Render();
    }

    protected override void OnDelete()
    {
        for (int i = 0; i < inputPorts.Count; i++)
            inputPorts[i].Delete();

        for (int i = 0; i < outputPorts.Count; i++)
            outputPorts[i].Delete();

        potConnectionWireUI?.Delete();

        Engine.NotifyDisconnectNodeAndUI(nodeId);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
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
}