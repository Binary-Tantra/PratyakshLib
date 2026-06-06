namespace RaylibNodeLibrary;

public abstract class Actor : EditorObject
{
    protected Actor(Drawable? parent) : base(parent) { }

    protected override bool InteractionUseWorldPos()
    {
        return true;
    }
}
