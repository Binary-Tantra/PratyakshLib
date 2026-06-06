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

    public Node(int inputPortCount, int outputPortCount) : base()
    {
        Engine.NotifyAddNode(Id);

        inputPorts = [];
        outputPorts = [];

        for (int i = 0; i < inputPortCount; i++)
        {
            Port p = new($"Input ({Id})", PortFlowType.Input);
            inputPorts.Add(p.Id, p);
        }

        for (int i = 0; i < outputPortCount; i++)
        {
            Port p = new($"Output ({Id})", PortFlowType.Output);
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
}
