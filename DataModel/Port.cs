namespace RaylibNodeLibrary.DataModel;

public enum PortFlowType
{
    Input, Output
}

public class Port : DataObject
{
    private string portName;
    private PortFlowType portFlowType;

    public string PortName { get => portName; }
    public PortFlowType PortFlowType { get => portFlowType; }

    public Port(string portName, PortFlowType portFlowType) : base()
    {
        Engine.NotifyAddPort(Id);

        this.portName = portName;
        this.portFlowType = portFlowType;
    }
}
