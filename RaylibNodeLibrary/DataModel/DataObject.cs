using Pratyaksh.Core;

namespace RaylibNodeLibrary.DataModel;

public class DataObject
{
    private readonly int id;

    public int Id { get => id; }

    public DataObject()
    {
        id = IdGen.GetNewID();
    }

    protected DataObject(int id)
    {
        this.id = id;
    }
}
