namespace Pratyaksh.Node.Core.DataModel;

public class Connection : DataObject
{
    private int sourcePortId;
    private int targetPortId;

    public int SourcePortId { get => sourcePortId; }
    public int TargetPortId { get => targetPortId; }

    public Connection(int sourcePortId, int targetPortId) : base()
    {
        this.sourcePortId = sourcePortId;
        this.targetPortId = targetPortId;
    }

    public Connection(int id, int sourcePortId, int targetPortId) : base(id)
    {
        this.sourcePortId = sourcePortId;
        this.targetPortId = targetPortId;
    }
}
