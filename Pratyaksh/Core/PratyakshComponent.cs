using System.Runtime.CompilerServices;

namespace Pratyaksh.Core;

public class PratyakshComponent : BaseObject
{
    protected PratyakshObject? ownerObject;

    public PratyakshObject? Owner { get => ownerObject; }

    public PratyakshComponent() { }

    internal PratyakshComponent(PratyakshObject owner)
    {
        SetOwner(owner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetOwner(PratyakshObject owner)
    {
        ownerObject = owner;
    }

    public override void Delete()
    {
        ownerObject = null;
    }
}

public class InvalidComponent : PratyakshComponent
{
    public InvalidComponent() : base() { }
}