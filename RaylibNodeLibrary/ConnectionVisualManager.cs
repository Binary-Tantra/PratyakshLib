using Pratyaksh.Core;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary;

public class ConnectionVisualManager : Actor
{
    private Dictionary<(int, int), ConnectionVisual> connectionUIs; // SourcePortUIId, TargetPortUIId

    public ConnectionVisualManager(Drawable? parent) : base(parent)
    {
        connectionUIs = [];
        ResetConnectionUIs();
    }

    private void DeleteConnectionUIs()
    {
        connectionUIs.ToList().ForEach(cuisKvp => { cuisKvp.Value.Delete(); });
        connectionUIs.Clear();
    }

    private void ResetConnectionUIs()
    {
        DeleteConnectionUIs();

        Dictionary<(int, int), Connection> connections = GEngine.Graph.Connections;
        List<(int, int)> connKeys = [.. connections.Keys];

        for (int i = 0; i < connKeys.Count; i++)
        {
            Connection c = connections[connKeys[i]];

            PortVisual? sp = GEngine.PortToPortUIDict[c.SourcePortId];
            PortVisual? tp = GEngine.PortToPortUIDict[c.TargetPortId];

            if (sp == null || tp == null)
            {
                Console.WriteLine("Error: The connected source port UI or target port UI was invalid while creating connection UI!");
                continue;
            }

            ConnectionVisual cui = new(sp, tp, this);
            connectionUIs.Add((sp.Id, tp.Id), cui);
        }
    }

    public void OnAddNewConnection(PortVisual outputPort, PortVisual inputPort)
    {
        if (connectionUIs.ContainsKey((outputPort.Id, inputPort.Id)))
            return;

        ConnectionVisual c = new(outputPort, inputPort, this);
        connectionUIs.Add((outputPort.Id, inputPort.Id), c);
    }

    public void OnRemoveConnection(PortVisual outputPort, PortVisual inputPort)
    {
        connectionUIs.Remove((outputPort.Id, inputPort.Id));
    }

    protected override void OnDraw()
    {
        connectionUIs.ToList().ForEach(cui => { cui.Value.Render(); });
    }

    protected override void OnDelete()
    {
        DeleteConnectionUIs();
    }
}
