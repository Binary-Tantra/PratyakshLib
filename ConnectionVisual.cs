namespace RaylibNodeLibrary;

public class ConnectionVisual : EditorObject
{
    private PortVisual startRef;
    private PortVisual endRef;

    private WireVisual connectionWireUI;

    public ConnectionVisual(PortVisual startRef, PortVisual endRef, Drawable? parent) : base(parent)
    {
        this.startRef = startRef;
        this.endRef = endRef;

        connectionWireUI = new WireVisual(this);
        connectionWireUI.SetStartPos(startRef);
        connectionWireUI.SetEndPos(endRef);
        connectionWireUI.Show();
    }

    protected override bool InteractionUseWorldPos()
    {
        return true;
    }

    protected override void OnDraw()
    {
        connectionWireUI.Render();
    }

    protected override void OnDelete()
    {
        connectionWireUI.Delete();
    }

    public (PortVisual start, PortVisual end) GetConnection()
    {
        return (startRef, endRef);
    }

    public PortVisual GetStart()
    {
        return startRef;
    }

    public PortVisual GetEnd()
    {
        return endRef;
    }
}
