namespace Pratyaksh.Node.Core.DataModel;

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

    public int TemplateId { get; set; } = -1;

    internal Node(int templateId, List<DataType> inputPortTypes, List<DataType> outputPortTypes, Action<int>? OnPortAdded) : base()
    {
        TemplateId = templateId;

        inputPorts = [];
        outputPorts = [];

        for (int i = 0; i < inputPortTypes.Count; i++)
        {
            Port p = new(inputPortTypes[i].Name, PortFlowType.Input, inputPortTypes[i]);
            OnPortAdded?.Invoke(p.Id);
            inputPorts.Add(p.Id, p);
        }

        for (int i = 0; i < outputPortTypes.Count; i++)
        {
            Port p = new(outputPortTypes[i].Name, PortFlowType.Output, outputPortTypes[i]);
            OnPortAdded?.Invoke(p.Id);
            outputPorts.Add(p.Id, p);
        }
    }

    internal Node(int id, int templateId) : base(id)
    {
        TemplateId = templateId;

        inputPorts = [];
        outputPorts = [];
    }

    public bool HasInputPort(int inputPortId)
    {
        return inputPorts.ContainsKey(inputPortId);
    }

    public bool HasOutputPort(int outputPortId)
    {
        return outputPorts.ContainsKey(outputPortId);
    }

    internal void AddInputPort(Port p)
    {
        inputPorts.Add(p.Id, p);
    }

    internal void AddOutputPort(Port p)
    {
        outputPorts.Add(p.Id, p);
    }

    internal void RemovePorts(Action<int>? onRemovePort)
    {
        List<int> keys = [.. inputPorts.Keys];
        for (int i = 0; i < keys.Count; i++)
        {
            if (inputPorts.Remove(keys[i]))
                onRemovePort?.Invoke(keys[i]);
        }

        keys = [.. outputPorts.Keys];
        for (int i = 0; i < keys.Count; i++)
        {
            if (outputPorts.Remove(keys[i]))
                onRemovePort?.Invoke(keys[i]);
        }
    }
}
