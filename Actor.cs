namespace RaylibNodeLibrary;

public abstract class Actor : EditorObject
{
    protected Actor(Drawable? parent) : base(parent) { }

    public override bool InteractionUseWorldPos()
    {
        return true;
    }
}
