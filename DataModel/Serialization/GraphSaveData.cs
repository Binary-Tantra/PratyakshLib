namespace RaylibNodeLibrary.DataModel.Serialization;

using System.Collections.Generic;
using System.Text.Json;

public class GraphSaveData
{
    public int MaxId { get; set; }
    public List<VariableSaveData> Variables { get; set; } = new();
    public List<NodeSaveData> Nodes { get; set; } = new();
    public List<ConnectionSaveData> Connections { get; set; } = new();
}

public class VariableSaveData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public JsonElement Value { get; set; }
}

public class NodeSaveData
{
    public int Id { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public List<PortSaveData> InputPorts { get; set; } = new();
    public List<PortSaveData> OutputPorts { get; set; } = new();
}

public class PortSaveData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FlowType { get; set; } // 0 for Input, 1 for Output
    public string DataTypeName { get; set; } = string.Empty;
}

public class ConnectionSaveData
{
    public int Id { get; set; }
    public int SourcePortId { get; set; }
    public int TargetPortId { get; set; }
}
