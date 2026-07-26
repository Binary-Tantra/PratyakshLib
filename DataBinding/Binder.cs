using System;

namespace RaylibNodeLibrary.DataBinding;

public abstract class BinderBase<T>
{
    protected bool isBound = false;
    public bool IsBound => isBound;

    public abstract BindableValueBase<T>? GetBoundValObject();
    public abstract BindableUIBase<T>? GetBoundUIObject();
    public abstract void NotifyBoundValOfChange(T newVal);
    public abstract void NotifyBoundUIOfChange(T newVal);
}

public class Binder<BindValueTarget, BindUITarget, BindValueType> : BinderBase<BindValueType>
    where BindValueTarget : BindableValueBase<BindValueType>
    where BindUITarget : BindableUIBase<BindValueType>
{
    private BindValueTarget? bindValueTarget = null;
    private BindUITarget? bindUITarget = null;

    public override BindableValueBase<BindValueType>? GetBoundValObject() => bindValueTarget;
    public override BindableUIBase<BindValueType>? GetBoundUIObject() => bindUITarget;

    public virtual void Bind(BindValueTarget valTarget, BindUITarget uiTarget)
    {
        if (isBound) Unbind();

        bindValueTarget = valTarget;
        bindUITarget = uiTarget;

        bindValueTarget.SetBinder(this);
        bindUITarget.SetBinder(this);

        bindValueTarget.NotifyBind();
        bindUITarget.NotifyBind();

        // Push initial data value from Data Model -> UI
        bindUITarget.Set(bindValueTarget.Get(), false);
        isBound = true;
    }

    public virtual void Unbind()
    {
        if (!isBound) return;

        bindValueTarget?.NotifyUnbind();
        bindUITarget?.NotifyUnbind();

        bindValueTarget?.UnsetBinder(this);
        bindUITarget?.UnsetBinder(this);

        isBound = false;

        bindUITarget?.SetDefaultNoNotify();

        bindValueTarget = null;
        bindUITarget = null;
    }

    public override void NotifyBoundValOfChange(BindValueType newVal)
    {
        if (!isBound || bindValueTarget == null) return;
        bindValueTarget.Set(newVal, false);
        bindValueTarget.onBoundUIChange?.Invoke(newVal);
    }

    public override void NotifyBoundUIOfChange(BindValueType newVal)
    {
        if (!isBound || bindUITarget == null) return;
        bindUITarget.Set(newVal, false);
        bindUITarget.onBoundValueChange?.Invoke(newVal);
    }

    public override string ToString()
    {
        return $"{GetType().Name} ({bindValueTarget}, {bindUITarget})";
    }
}
