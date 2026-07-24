namespace RaylibNodeLibrary.DataModel.Serialization;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

public static class GraphSerializer
{
    public static string Serialize(Graph graph, Dictionary<int, NodeVisual?> nodeVisuals, int maxId)
    {
        GraphSaveData data = new GraphSaveData
        {
            MaxId = maxId
        };

        foreach (var v in graph.Variables.Values)
        {
            data.Variables.Add(new VariableSaveData
            {
                Id = v.Id,
                Name = v.VarName,
                TypeName = v.VarType.Name,
                Value = JsonSerializer.SerializeToElement(v.VarValue)
            });
        }

        foreach (var n in graph.Nodes.Values)
        {
            NodeSaveData nd = new NodeSaveData
            {
                Id = n.Id,
                TemplateName = n.TemplateName
            };

            if (nodeVisuals.TryGetValue(n.Id, out NodeVisual? vis) && vis != null)
            {
                nd.PositionX = vis.RelativePosition.X;
                nd.PositionY = vis.RelativePosition.Y;
            }

            foreach (var p in n.InputPorts.Values)
            {
                nd.InputPorts.Add(new PortSaveData
                {
                    Id = p.Id,
                    Name = p.PortName,
                    FlowType = (int)p.PortFlowType,
                    DataTypeName = p.DataType.Name
                });
            }

            foreach (var p in n.OutputPorts.Values)
            {
                nd.OutputPorts.Add(new PortSaveData
                {
                    Id = p.Id,
                    Name = p.PortName,
                    FlowType = (int)p.PortFlowType,
                    DataTypeName = p.DataType.Name
                });
            }

            data.Nodes.Add(nd);
        }

        foreach (var c in graph.Connections.Values)
        {
            data.Connections.Add(new ConnectionSaveData
            {
                Id = c.Id,
                SourcePortId = c.SourcePortId,
                TargetPortId = c.TargetPortId
            });
        }

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(data, options);
    }

    public static GraphSaveData? Deserialize(string json)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<GraphSaveData>(json, options);
    }
}
