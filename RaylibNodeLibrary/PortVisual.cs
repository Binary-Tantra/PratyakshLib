using System.Numerics;
using Pratyaksh.Core;
using Pratyaksh.UI;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary;

public class PortVisual : Actor, IPointerVisitable, IPointerInteractable, IDragable
{
    private readonly int portId;

    private PortFlowType portFlowType;

    private float portSize = 5f;
    private string portName;

    private Raylib_cs.Color portColor = Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.55f);
    public Raylib_cs.Color PortColor { get => portColor; }
    
    private bool isExecution;
    public bool IsExecution { get => isExecution; }
    private Raylib_cs.Color portTextColor = Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.65f);

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

    public override Rectangle InteractionRect => new Rectangle(Position.X + portInteractionRelativeRect.X, Position.Y + portInteractionRelativeRect.Y, portInteractionRelativeRect.Width, portInteractionRelativeRect.Height);

    public static int GetPortTSize(string name)
    {
        return LayoutEngine.MeasureTextW(name, portTFontSize);
    }

    public bool IsConnected
    {
        get
        {
            Node? node = GEngine.Graph.GetNode(ParentNodeId);
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
        DataType? dataType = GEngine.Graph.Types.GetType(dataTypeId);
        if (dataType != null)
        {
            isExecution = dataType.Category.HasFlag(DataCategory.Execution);

            if (GEngine.DataTypeColors.TryGetValue(dataTypeId, out Raylib_cs.Color c))
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

        if (GEngine.DataTypeColors.TryGetValue(dataTypeId, out Raylib_cs.Color c))
            portColor = c;
            
        DataType? dataType = GEngine.Graph.Types.GetType(dataTypeId);
        isExecution = dataType != null && dataType.Category.HasFlag(DataCategory.Execution);

        GEngine.NotifyConnectPortAndUI(portId, this);
        
        this.portFlowType = portFlowType;

        selfInteractable = true;

        RelativePosition = portRelativeLocation;
        
        this.portName = portName;

        portTSize = GetPortTSize(portName);

        if (Parent != null)
            parentNodeUI = (NodeVisual)Parent;

        int hoverRectFlowOffset = portFlowType == PortFlowType.Input ? 0 : -(portTSize + 5);
        portInteractionRelativeRect = new(-portSize + hoverRectFlowOffset, -portSize - 2, portTSize + portSize * 2 + 10, portSize * 2 + 5);
    }

    protected override void OnDraw()
    {
        if (isHovered)
            Raylib_cs.Raylib.DrawRectangle((int)(Position.X + portInteractionRelativeRect.X), (int)(Position.Y + portInteractionRelativeRect.Y), (int)portInteractionRelativeRect.Width, (int)portInteractionRelativeRect.Height, Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.4f));

        bool isConnected = IsConnected;
        Raylib_cs.Color fillColor = Raylib_cs.Raylib.Fade(portColor, 0.4f);

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
                Raylib_cs.Raylib.DrawTriangle(p1, p2, p3, fillColor);

            Raylib_cs.Raylib.DrawTriangleLines(p1, p2, p3, portColor);
        }
        else
        {
            if (isConnected)
                Raylib_cs.Raylib.DrawCircle((int)Position.X, (int)Position.Y, portSize, fillColor);

            Raylib_cs.Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, portSize, portColor);
        }

        if (!isExecution)
        {
            if (portFlowType == PortFlowType.Input)
                LayoutEngine.DrawTextAbsolute(portName, (int)Position.X + 10, (int)Position.Y - 5, portTextColor, portTFontSize, Vector2.Zero);
            else if (portFlowType == PortFlowType.Output)
                LayoutEngine.DrawTextAbsolute(portName, (int)Position.X - portTSize - 10, (int)Position.Y - 5, portTextColor, portTFontSize, Vector2.Zero);
        }
    }

    protected override void OnDelete()
    {
        GEngine.NotifyDisconnectPortAndUI(portId);
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
        if (evt.MouseButton != MouseButton.Left)
            return false;

        Engine.Instance.InteractionManager.CapturePointer(this);
        parentNodeUI?.UIConnectionStart(this);
        return true;
    }

    public void OnDrag(PointerInteractEventData evt)
    {
        parentNodeUI?.UIConnectionMove(evt);
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (Engine.Instance.InteractionManager.CurrentlyHovered is PortVisual portUI)
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

                Engine.Instance.InteractionManager.ReleasePointer();
                return true;
            }
        }

        parentNodeUI?.UIConnectionCanceled(this);
        Engine.Instance.InteractionManager.ReleasePointer();
        return true;
    }

    public void OnMouseExit(PointerVisitEventData evt)
    {
        isHovered = false;
    }
}
