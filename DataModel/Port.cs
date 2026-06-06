namespace RaylibNodeLibrary.DataModel;

public enum PortFlowType
{
    Input, Output
}

public class Port : DataObject
{
    private PortFlowType portFlowType;

    public PortFlowType PortFlowType { get => portFlowType; }

    public Port(PortFlowType portFlowType) : base()
    {
        Engine.NotifyAddPort(Id);

        this.portFlowType = portFlowType;
    }
}
