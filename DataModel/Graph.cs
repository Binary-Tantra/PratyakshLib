namespace RaylibNodeLibrary.DataModel;

public class Graph
{
    private Dictionary<int, Node> graphNodes; // id, Node ref
    private Dictionary<(int, int), Connection> graphConnections; // (sourcePortId, targetPortId), Connection ref
    private Dictionary<int, Variable> graphVariables; // id, Variable ref

    public Dictionary<int, Variable> Variables { get => graphVariables; }
    public Dictionary<(int, int), Connection> Connections { get => graphConnections; }

    public Graph()
    {
        graphNodes = [];
        graphConnections = [];
        graphVariables = [];
    }

    public Node AddNode(int inputPortCount, int outputPortCount)
    {
        Node n = new(inputPortCount, outputPortCount);
        graphNodes.Add(n.Id, n);

        return n;
    }

    public void RemoveNode(int id)
    {
        if (!graphNodes.TryGetValue(id, out Node? n))
        {
            Console.WriteLine($"Error: Cannot Remove node with the id {id}. The node doesn't exist in the graph.");
            return;
        }

        if (n == null)
        {
            Console.WriteLine($"Error: Cannot Remove node with the id {id}. The node exists but is invalid.");
            return;
        }

        List<int> inputPortIds = n.InputPortIds;
        List<int> outputPortIds = n.OutputPortIds;

        List<(int sourcePort, int targetPort)> kvp = [.. graphConnections.Keys];
        HashSet<int> kvpMarked = [];

        for (int i = 0; i < kvp.Count; i++)
        {
            int sourceP = kvp[i].sourcePort;
            int targetP = kvp[i].targetPort;

            for (int p = 0; p < inputPortIds.Count; p++)
            {
                if (inputPortIds[p] == targetP)
                    kvpMarked.Add(i);
            }

            for (int p = 0; p < outputPortIds.Count; p++)
            {
                if (outputPortIds[p] == sourceP)
                    kvpMarked.Add(i);
            }
        }

        List<int> kvpIdxsToRemove = [.. kvpMarked];

        for (int i = 0; i < kvpIdxsToRemove.Count; i++)
        {
            (int sourcePort, int targetPort) kvpI = kvp[kvpIdxsToRemove[i]];
            graphConnections.Remove(kvpI);
            Engine.NotifyRemoveConnection(kvpI.sourcePort, kvpI.targetPort);
        }

        graphNodes[id].RemovePorts();

        if (graphNodes.Remove(id))
            Engine.NotifyRemoveNode(id);
    }

    public Node? GetNode(int nodeId)
    {
        if (!graphNodes.TryGetValue(nodeId, out Node? node))
        {
            Console.WriteLine($"Error: Cannot get node with id: {nodeId}. The node doesn't exist. ");
            return null;
        }

        return node;
    }

    public bool AddConnection(int sourceNodeId, int sourcePortId, int targetNodeId, int targetPortId)
    {
        Node? n = GetNode(sourceNodeId);

        if (n == null)
            return false;

        Node sourceNode = n;

        n = GetNode(targetNodeId);

        if (n == null)
            return false;

        Node targetNode = n;

        if (!sourceNode.HasOutputPort(sourcePortId))
            return false;

        if (!targetNode.HasInputPort(targetPortId))
            return false;

        Connection c = new(sourcePortId, targetPortId);
        graphConnections.Add((sourcePortId, targetPortId), c);

        return true;
    }

    public void RemoveConnection(int sourcePortId, int targetPortId)
    {
        if (!graphConnections.ContainsKey((sourcePortId, targetPortId)))
        {
            Console.WriteLine($"Error: Cannot Remove connection with the id ({sourcePortId}, {targetPortId}). The connection doesn't exist.");
            return;
        }

        graphConnections.Remove((sourcePortId, targetPortId));
    }

    public Connection? GetConnection(int sourcePortId, int targetPortId)
    {
        if (!graphConnections.TryGetValue((sourcePortId, targetPortId), out Connection? connection))
        {
            Console.WriteLine($"Error: Cannot get variable with id: ({sourcePortId}, {targetPortId}). The variable doesn't exist. ");
            return null;
        }

        return connection;
    }

    public void AddVariable(string name)
    {
        Variable newV = new(name, (object)0);
        graphVariables.Add(newV.Id, newV);
    }

    public void RemoveVariable(int id)
    {
        if (!graphVariables.ContainsKey(id))
        {
            Console.WriteLine($"Error: Cannot Remove variable with the id {id}. The variable doesn't exist.");
            return;
        }

        graphVariables.Remove(id);
    }

    public Variable? GetVariable(int varId)
    {
        if (!graphVariables.TryGetValue(varId, out Variable? variable))
        {
            Console.WriteLine($"Error: Cannot get variable with id: {varId}. The variable doesn't exist. ");
            return null;
        }

        return variable;
    }

    public bool RenameVariable(int varId, string newName)
    {
        Variable? var = GetVariable(varId);
        var?.SetName_Graph(newName);

        return var != null;
    }
}
