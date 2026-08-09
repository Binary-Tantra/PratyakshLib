namespace Pratyaksh.Core.DataBinding;

public abstract class BindableUIBase<T> : BindableBase<T>
{
    public Action<T>? onBoundValueChange;

    public override void Set(T newVal, bool notifyBound)
    {
        OnSet(newVal);
        if (notifyBound)
            NotifyBoundValOfChange(newVal);
    }

    public void SetDefaultNoNotify()
    {
        Set(GetDefault(), false);
    }

    protected abstract void OnSet(T newVal);
    protected abstract T GetDefault();

    public void NotifyBoundValOfChange(T data)
    {
        for (int i = 0; i < Binders.Count; i++)
        {
            Binders[i].NotifyBoundValOfChange(data);
            Binders[i].GetBoundValObject()?.NotifyBoundUIOfChange(data);
        }
    }
}
