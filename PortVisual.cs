using System.Numerics;
using Raylib_cs;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary;

public class PortVisual : Actor, IPointerVisitable, IPointerInteractable, IDragable
{
    private readonly int portId;

    private PortFlowType portFlowType;

    private float portSize = 5f;
    private string portName;

    private Color portColor = Raylib.Fade(Color.White, 0.55f);
    public Color PortColor { get => portColor; }
    
    private bool isExecution;
    public bool IsExecution { get => isExecution; }
    private Color portTextColor = Raylib.Fade(Color.White, 0.65f);

    private Rectangle portInteractionRelativeRect;

    private static int portTFontSize = 10;

    private int portTSize;

    private bool isHovered = false;

    private NodeVisual? parentNodeUI;

    public int PortId { get => portId; }
    
    public int ParentNodeId
    {
        get => parentNodeUI == null ? -1 : parentNodeUI.NodeId;
    }
    public int PortTSize { get => portTSize; }

    public static int GetPortTSize(string name)
    {
        return Raylib.MeasureText(name, portTFontSize);
    }

    public bool IsConnected
    {
        get
        {
            Node? node = Engine.Graph.GetNode(ParentNodeId);
            if (node == null) return false;
            
            if (portFlowType == PortFlowType.Input && node.InputPorts.ContainsKey(portId))
                return node.InputPorts[portId].IsConnected;
            if (portFlowType == PortFlowType.Output && node.OutputPorts.ContainsKey(portId))
                return node.OutputPorts[portId].IsConnected;
                
            return false;
        }
    }

    public void UpdateDataType(int dataTypeId, string? newPortName = null)
    {
        DataType? dataType = Engine.Graph.Types.GetType(dataTypeId);
        if (dataType != null)
        {
            isExecution = dataType.Category.HasFlag(DataCategory.Execution);

            if (Engine.DataTypeColors.TryGetValue(dataTypeId, out Color c))
                portColor = c;
            
            if (newPortName != null)
            {
                portName = newPortName;
                portTSize = GetPortTSize(portName);
            }
        }
    }

    public PortVisual(int portId, PortFlowType portFlowType, Vector2 portRelativeLocation, string portName, int dataTypeId, Drawable? parent = null) : base(parent)
    {
        this.portId = portId;

        if (Engine.DataTypeColors.TryGetValue(dataTypeId, out Color c))
            portColor = c;
            
        DataType? dataType = Engine.Graph.Types.GetType(dataTypeId);
        isExecution = dataType != null && dataType.Category.HasFlag(DataCategory.Execution);

        Engine.NotifyConnectPortAndUI(portId, this);
        
        this.portFlowType = portFlowType;

        selfInteractable = true;

        relativePosition = portRelativeLocation;
        
        this.portName = portName;

        portTSize = GetPortTSize(portName);

        if (Parent != null)
            parentNodeUI = (NodeVisual)Parent;

        int hoverRectFlowOffset = portFlowType == PortFlowType.Input ? 0 : -(portTSize + 5);
        portInteractionRelativeRect = new(-portSize + hoverRectFlowOffset, -portSize - 2, portTSize + portSize * 2 + 10, portSize * 2 + 5);
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X + portInteractionRelativeRect.X, Position.Y + portInteractionRelativeRect.Y, portInteractionRelativeRect.Width, portInteractionRelativeRect.Height); ;
    }

    protected override void OnDraw()
    {
        if (isHovered)
            Raylib.DrawRectangle((int)(Position.X + portInteractionRelativeRect.X), (int)(Position.Y + portInteractionRelativeRect.Y), (int)portInteractionRelativeRect.Width, (int)portInteractionRelativeRect.Height, Raylib.Fade(Color.White, 0.4f));

        bool isConnected = IsConnected;
        Color fillColor = Raylib.Fade(portColor, 0.4f);

        if (isExecution)
        {
            Vector2 p1, p2, p3;
            if (portFlowType == PortFlowType.Input)
            {
                p1 = new Vector2(Position.X - portSize, Position.Y - portSize);
                p2 = new Vector2(Position.X - portSize, Position.Y + portSize);
                p3 = new Vector2(Position.X + portSize, Position.Y);
            }
            else
            {
                p1 = new Vector2(Position.X - portSize, Position.Y - portSize);
                p2 = new Vector2(Position.X - portSize, Position.Y + portSize);
                p3 = new Vector2(Position.X + portSize, Position.Y);
            }
            
            if (isConnected)
                Raylib.DrawTriangle(p1, p2, p3, fillColor);
            
            Raylib.DrawTriangleLines(p1, p2, p3, portColor);
        }
        else
        {
            if (isConnected)
                Raylib.DrawCircle((int)Position.X, (int)Position.Y, portSize, fillColor);
                
            Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, portSize, portColor);
        }

        if (portFlowType == PortFlowType.Input)
            Raylib.DrawText(portName, (int)Position.X + 10, (int)Position.Y - 5, portTFontSize, portTextColor);
        else if (portFlowType == PortFlowType.Output)
            Raylib.DrawText(portName, (int)Position.X - portTSize - 10, (int)Position.Y - 5, portTFontSize, portTextColor);
    }

    protected override void OnDelete()
    {
        Engine.NotifyDisconnectPortAndUI(portId);
    }

    public void OnMouseEnter(PointerVisitEventData evt)
    {
        isHovered = true;
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
        parentNodeUI?.UIConnectionStart(this);
        return true;
    }

    public void OnDrag(PointerInteractEventData evt)
    {
        parentNodeUI?.UIConnectionMove(evt);
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (InteractionManager.CurrentlyHovered is PortVisual portUI)
        {
            if (portUI != this &&
                portUI.Parent != null &&
                portUI.Parent != Parent &&
                portUI.portFlowType != portFlowType)
            {
                if (portFlowType == PortFlowType.Output)
                    parentNodeUI?.UIConnectionComplete(this, portUI);
                else
                    parentNodeUI?.UIConnectionComplete(portUI, this);

                InteractionManager.ReleasePointer();
                return true;
            }
        }

        parentNodeUI?.UIConnectionCanceled(this);
        InteractionManager.ReleasePointer();
        return true;
    }

    public void OnMouseExit(PointerVisitEventData evt)
    {
        isHovered = false;
    }
}
