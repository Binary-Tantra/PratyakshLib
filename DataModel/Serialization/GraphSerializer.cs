namespace RaylibNodeLibrary.DataModel.Serialization;

using System.Collections.Generic;
using System.Text.Json;

using Raylib_cs;
using RaylibNodeLibrary.UI;

public static class GraphSerializer
{
    private static JsonSerializerOptions? serializeOptions;
    private static JsonSerializerOptions? deserializeOptions;

    public static string Serialize(Graph graph, Dictionary<int, NodeVisual?> nodeVisuals, NodeRegistry nodeRegistry, int maxId)
    {
        GraphSaveData data = new GraphSaveData
        {
            MaxId = maxId
        };

        foreach (var template in nodeRegistry.AllTemplates)
        {
            var templateSaveData = new NodeTemplateSaveData
            {
                Id = template.Id,
                Name = template.Name,
                Category = template.Category,
                InputPortTypeNames = [.. template.InputPortTypeNames],
                OutputPortTypeNames = [.. template.OutputPortTypeNames],
                Payload = template.Payload != null ? JsonSerializer.SerializeToElement(template.Payload) : null
            };

            foreach (var (elemType, elemDesc) in template.UIElements)
            {
                templateSaveData.UIElements.Add(SerializeUIElement(elemType, elemDesc));
            }

            data.NodeTemplates.Add(templateSaveData);
        }

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
            NodeSaveData nd = new()
            {
                Id = n.Id,
                TemplateId = n.TemplateId
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

        serializeOptions ??= new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(data, serializeOptions);
    }

    private static UIElementSaveData SerializeUIElement(UIElementType elemType, UIElementDescription elemDesc)
    {
        var saveData = new UIElementSaveData
        {
            ElementType = (int)elemType,
            Text = elemDesc.text ?? string.Empty
        };

        if (elemDesc is TextDesc textDesc)
        {
            saveData.ColorR = textDesc.color.R;
            saveData.ColorG = textDesc.color.G;
            saveData.ColorB = textDesc.color.B;
            saveData.ColorA = textDesc.color.A;
        }
        else if (elemDesc is RectUIEDescription rectDesc)
        {
            saveData.Width = rectDesc.width;
            saveData.Height = rectDesc.height;

            if (rectDesc is InputFieldDesc inputDesc)
            {
                saveData.PlaceholderText = inputDesc.placeholderText ?? string.Empty;
            }
            else if (rectDesc is SelectableDesc selectableDesc)
            {
                saveData.StartingState = selectableDesc.startingSelected;
            }
            else if (rectDesc is ToggleDesc toggleDesc)
            {
                saveData.StartingState = toggleDesc.startingState;
            }
            else if (rectDesc is DropdownDesc dropDesc)
            {
                saveData.Options = dropDesc.options != null ? [.. dropDesc.options] : [];
                saveData.SelectedIndex = dropDesc.selectedIndex;
            }
            else if (rectDesc is HorizontalGroupDesc groupDesc)
            {
                saveData.Spacing = groupDesc.spacing;
                if (groupDesc.uiElements != null)
                {
                    foreach (var child in groupDesc.uiElements)
                    {
                        saveData.Children.Add(SerializeUIElement(child.elemType, child.elemDesc));
                    }
                }
            }
        }

        return saveData;
    }

    public static (UIElementType elemType, UIElementDescription elemDesc) DeserializeUIElement(UIElementSaveData data)
    {
        UIElementType elemType = (UIElementType)data.ElementType;
        UIElementDescription elemDesc;

        switch (elemType)
        {
            case UIElementType.Text:
                Color col = new Color(data.ColorR, data.ColorG, data.ColorB, data.ColorA);
                elemDesc = new TextDesc(data.Text, col);
                break;

            case UIElementType.Button:
                elemDesc = new ButtonDesc(data.Text, data.Width, data.Height, (btn) => { });
                break;

            case UIElementType.InputField:
                elemDesc = new InputFieldDesc(data.PlaceholderText, data.Text, data.Width, data.Height);
                break;

            case UIElementType.Selectable:
                elemDesc = new SelectableDesc(data.Text, data.StartingState, data.Width, data.Height, (sel) => { });
                break;

            case UIElementType.Toggle:
                elemDesc = new ToggleDesc(data.Text, data.StartingState, data.Width, data.Height, (tog) => { });
                break;

            case UIElementType.Dropdown:
                elemDesc = new DropdownDesc([.. data.Options], data.SelectedIndex, data.Width, data.Height, (dd) => { });
                break;

            case UIElementType.Group:
                var children = new List<(UIElementType, UIElementDescription)>();
                if (data.Children != null)
                {
                    foreach (var childData in data.Children)
                    {
                        children.Add(DeserializeUIElement(childData));
                    }
                }
                elemDesc = new HorizontalGroupDesc(data.Text, data.Spacing, children, data.Width, data.Height);
                break;

            default:
                elemDesc = new TextDesc(data.Text, Color.White);
                break;
        }

        return (elemType, elemDesc);
    }

    public static GraphSaveData? Deserialize(string json)
    {
        deserializeOptions ??= new() { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<GraphSaveData>(json, deserializeOptions);
    }
}
