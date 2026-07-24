namespace RaylibNodeLibrary.DataModel;

public enum PortFlowType
{
    Input, Output
}

public class Port : DataObject
{
    private string portName;
    private PortFlowType portFlowType;
    private DataType dataType;

    public string PortName { get => portName; set => portName = value; }
    public PortFlowType PortFlowType { get => portFlowType; }
    public DataType DataType { get => dataType; set => dataType = value; }
    public bool IsConnected { get; set; }

    public Port(string portName, PortFlowType portFlowType, DataType dataType) : base()
    {
        Engine.NotifyAddPort(Id);

        this.portName = portName;
        this.portFlowType = portFlowType;
        this.dataType = dataType;
    }

    public Port(int id, string portName, PortFlowType portFlowType, DataType dataType) : base(id)
    {
        Engine.NotifyAddPort(Id);

        this.portName = portName;
        this.portFlowType = portFlowType;
        this.dataType = dataType;
    }
}
