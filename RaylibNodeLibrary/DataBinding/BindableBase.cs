namespace RaylibNodeLibrary.DataBinding;

public abstract class BindableBase { }

public abstract class BindableBase<T> : BindableBase
{
    private List<BinderBase<T>> binders = new();
    public List<BinderBase<T>> Binders => binders;

    public void SetBinder(BinderBase<T> binder)
    {
        if (!binders.Contains(binder))
            binders.Add(binder);
    }

    public void UnsetBinder(BinderBase<T> binder)
    {
        binders.Remove(binder);
    }

    public abstract T Get();
    public abstract void Set(T newVal, bool notifyBound);
    public abstract void NotifyBind();
    public abstract void NotifyUnbind();
}

public abstract class BindableValueBase<T> : BindableBase<T>
{
    protected T data = default!;
    public Action<T>? onBoundUIChange;

    public override T Get() => data;

    public override void Set(T newVal, bool notifyBound)
    {
        OnSet(newVal);
        if (notifyBound)
            NotifyBoundUIOfChange(newVal);
    }

    protected virtual void OnSet(T newVal) => data = newVal;

    public void NotifyBoundUIOfChange(T data)
    {
        for (int i = 0; i < Binders.Count; i++)
            Binders[i].NotifyBoundUIOfChange(data);
    }

    public override void NotifyBind() { }
    public override void NotifyUnbind() { }
}

public class BindableValue<T> : BindableValueBase<T>
{
    public BindableValue(T initialData) { this.data = initialData; }
}

public class BindableBool : BindableValueBase<bool>
{
    public BindableBool(bool initial = false) { data = initial; }
}

public class BindableInt : BindableValueBase<int>
{
    public BindableInt(int initial = 0) { data = initial; }
}

public class BindableFloat : BindableValueBase<float>
{
    public BindableFloat(float initial = 0f) { data = initial; }
}

public class BindableString : BindableValueBase<string>
{
    public BindableString(string initial = "") { data = initial ?? ""; }
}
