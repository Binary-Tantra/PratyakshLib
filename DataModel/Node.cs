namespace RaylibNodeLibrary.DataModel;

public class Node : DataObject
{
    private Dictionary<int, Port> inputPorts;
    private Dictionary<int, Port> outputPorts;

    public Dictionary<int, Port> InputPorts { get => inputPorts; }
    public Dictionary<int, Port> OutputPorts { get => outputPorts; }

    public List<int> InputPortIds { get => [.. inputPorts.Select((p) => p.Key)]; }
    public List<int> OutputPortIds { get => [.. outputPorts.Select((p) => p.Key)]; }

    public List<(int, string)> InputPortIdNames { get => [.. inputPorts.Select((p) => (p.Key, p.Value.PortName))]; }
    public List<(int, string)> OutputPortIdNames { get => [.. outputPorts.Select((p) => (p.Key, p.Value.PortName))]; }

    public Node(List<DataType> inputPortTypes, List<DataType> outputPortTypes) : base()
    {
        Engine.NotifyAddNode(Id);

        inputPorts = [];
        outputPorts = [];

        for (int i = 0; i < inputPortTypes.Count; i++)
        {
            Port p = new(inputPortTypes[i].Name, PortFlowType.Input, inputPortTypes[i]);
            inputPorts.Add(p.Id, p);
        }

        for (int i = 0; i < outputPortTypes.Count; i++)
        {
            Port p = new(outputPortTypes[i].Name, PortFlowType.Output, outputPortTypes[i]);
            outputPorts.Add(p.Id, p);
        }
    }

    public bool HasInputPort(int inputPortId)
    {
        return inputPorts.ContainsKey(inputPortId);
    }

    public bool HasOutputPort(int outputPortId)
    {
        return outputPorts.ContainsKey(outputPortId);
    }

    public void RemovePorts()
    {
        List<int> keys = [.. inputPorts.Keys];
        for (int i = 0; i < keys.Count; i++)
        {
            if (inputPorts.Remove(keys[i]))
                Engine.NotifyRemovePort(keys[i]);
        }

        keys = [.. outputPorts.Keys];
        for (int i = 0; i < keys.Count; i++)
        {
            if (outputPorts.Remove(keys[i]))
                Engine.NotifyRemovePort(keys[i]);
        }
    }

    public void AddInputPort(DataType dataType)
    {
        Port newI = new(dataType.Name, PortFlowType.Input, dataType);
        inputPorts.Add(newI.Id, newI);
        Engine.NotifyUpdateNode(Id);
    }

    public void AddOutputPort(DataType dataType)
    {
        Port newO = new(dataType.Name, PortFlowType.Output, dataType);
        outputPorts.Add(newO.Id, newO);
        Engine.NotifyUpdateNode(Id);
    }
}
