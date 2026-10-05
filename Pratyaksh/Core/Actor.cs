namespace Pratyaksh.Core;

public abstract class Actor : EditorObject
{
    protected Actor(Transform? parent) : base(parent) { }

    public override bool InteractionUseWorldPos()
    {
        return true;
    }

    public override void Render()
    {
        base.Render();
        OnDraw();
    }

    protected abstract void OnDraw();
}
