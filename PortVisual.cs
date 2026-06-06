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
    private Color portTextColor = Raylib.Fade(Color.White, 0.65f);

    private Rectangle portInteractionRelativeRect;

    private int portTFontSize = 10;

    private int portTSize;

    private bool isHovered = false;

    private NodeVisual? parentNodeUI;

    public int PortId { get => portId; }
    
    public int ParentNodeId
    {
        get => parentNodeUI == null ? -1 : parentNodeUI.NodeId;
    }

    public PortVisual(int portId, PortFlowType portFlowType, Vector2 portRelativeLocation, string portName = "Port", Drawable? parent = null) : base(parent)
    {
        this.portId = portId;

        Engine.NotifyConnectPortAndUI(portId, this);
        
        this.portFlowType = portFlowType;

        selfInteractable = true;

        relativePosition = portRelativeLocation;
        
        this.portName = portName;

        portTFontSize = 10;
        portTSize = Raylib.MeasureText(portName, portTFontSize);

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

        Raylib.DrawCircleLines((int)Position.X, (int)Position.Y, portSize, portColor);

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
