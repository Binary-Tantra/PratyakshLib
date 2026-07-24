namespace RaylibNodeLibrary;

using System.Collections.Generic;
using System.Numerics;
using RaylibNodeLibrary.DataModel;

public class NodeRegistry
{
    private Dictionary<string, NodeTemplate> templatesByName = [];
    private Dictionary<string, List<NodeTemplate>> templatesByCategory = [];

    public IReadOnlyList<NodeTemplate> AllTemplates => [.. templatesByName.Values];

    public void RegisterNode(NodeTemplate template)
    {
        templatesByName[template.Name] = template;

        if (!templatesByCategory.ContainsKey(template.Category))
            templatesByCategory[template.Category] = [];
        
        if (!templatesByCategory[template.Category].Contains(template))
            templatesByCategory[template.Category].Add(template);
    }

    public void UnregisterNode(string name)
    {
        if (templatesByName.TryGetValue(name, out var template))
        {
            templatesByName.Remove(name);
            if (templatesByCategory.ContainsKey(template.Category))
            {
                templatesByCategory[template.Category].Remove(template);
                if (templatesByCategory[template.Category].Count == 0)
                {
                    templatesByCategory.Remove(template.Category);
                }
            }
        }
    }

    public void UnregisterNodeByPayload(object payload)
    {
        string? nameToRemove = null;
        foreach (var template in templatesByName.Values)
        {
            if (template.Payload != null && template.Payload.Equals(payload))
            {
                nameToRemove = template.Name;
                break;
            }
        }
        if (nameToRemove != null) UnregisterNode(nameToRemove);
    }

    public NodeTemplate? GetTemplate(string name)
    {
        return templatesByName.TryGetValue(name, out var template) ? template : null;
    }

    public IReadOnlyList<NodeTemplate> GetTemplatesInCategory(string category)
    {
        return templatesByCategory.TryGetValue(category, out var list) ? list : [];
    }

    public IReadOnlyList<string> GetCategories()
    {
        return [.. templatesByCategory.Keys];
    }
    
    public NodeVisual? SpawnNode(Graph graph, string templateName, Vector2 position)
    {
        NodeTemplate? template = GetTemplate(templateName);
        if (template == null) return null;
        
        List<DataType> inPorts = [];
        foreach (var typeName in template.InputPortTypeNames)
        {
            DataType? t = graph.Types.GetType(typeName);
            if (t != null) inPorts.Add(t);
        }
        
        List<DataType> outPorts = [];
        foreach (var typeName in template.OutputPortTypeNames)
        {
            DataType? t = graph.Types.GetType(typeName);
            if (t != null) outPorts.Add(t);
        }

        Node n = graph.AddNode(template.Name, inPorts, outPorts);
        
        NodeVisual nodeVis = new(n.Id, template.UIElements, template.Name, position.X, position.Y);
        return nodeVis;
    }
}
