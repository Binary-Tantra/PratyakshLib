namespace Pratyaksh.Core;

public abstract class BaseObject
{
    private readonly int id;

    public int Id { get => id; }

    public Action OnDeleteObject;

    public BaseObject()
    {
        id = IdGen.GetNewID();
        OnDeleteObject = () => { };
    }

    public abstract void Delete();
}
