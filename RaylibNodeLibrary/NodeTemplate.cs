using RaylibNodeLibrary.DataModel;
using RaylibNodeLibrary.UI;

namespace RaylibNodeLibrary;

public class NodeTemplate
{
    public int Id { get; }
    public string Name { get; }
    public string Category { get; }
    
    public List<string> InputPortTypeNames { get; }
    public List<string> OutputPortTypeNames { get; }
    public List<(UIElementType elemType, UIElementDescription elemDesc)> UIElements { get; }
    public object? Payload { get; }

    public NodeTemplate(string name, string category, 
                        List<string> inputPortTypeNames, 
                        List<string> outputPortTypeNames, 
                        List<(UIElementType, UIElementDescription)> uiElements,
                        object? payload = null) 
        : this(IdGen.GetNewID(), name, category, inputPortTypeNames, outputPortTypeNames, uiElements, payload)
    {
    }

    public NodeTemplate(int id, string name, string category, 
                        List<string> inputPortTypeNames, 
                        List<string> outputPortTypeNames, 
                        List<(UIElementType, UIElementDescription)> uiElements,
                        object? payload = null)
    {
        Id = id;
        Name = name;
        Category = category;
        InputPortTypeNames = inputPortTypeNames;
        OutputPortTypeNames = outputPortTypeNames;
        UIElements = uiElements;
        Payload = payload;
    }
}
