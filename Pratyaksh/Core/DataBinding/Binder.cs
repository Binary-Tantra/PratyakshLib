namespace Pratyaksh.Core.DataBinding;

public abstract class BinderBase
{
    public abstract void Bind(bool unbindCheck = true);
    public abstract void Unbind();
}

public abstract class BinderBase<T> : BinderBase
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

    public void SetBindTargets(BindValueTarget valTarget, BindUITarget uiTarget)
    {
        bindValueTarget = valTarget;
        bindUITarget = uiTarget;
    }

    public override void Bind(bool unbindCheck = true)
    {
        if (unbindCheck && isBound) Unbind();

        if (bindValueTarget == null || bindUITarget == null)
        {
            Console.WriteLine($"Invalid binding targets: ValueTarget: {bindValueTarget}, UITarget: {bindUITarget}");
            return;
        }

        bindValueTarget.SetBinder(this);
        bindUITarget.SetBinder(this);

        bindValueTarget.NotifyBind();
        bindUITarget.NotifyBind();

        // Push initial data value from Data Model -> UI
        bindUITarget.Set(bindValueTarget.Get(), false);
        isBound = true;
    }

    public virtual void Bind(BindValueTarget valTarget, BindUITarget uiTarget)
    {
        if (isBound) Unbind();

        SetBindTargets(valTarget, uiTarget);
        Bind(false);
    }

    public override void Unbind()
    {
        if (!isBound) return;

        bindValueTarget?.NotifyUnbind();
        bindUITarget?.NotifyUnbind();

        bindValueTarget?.UnsetBinder(this);
        bindUITarget?.UnsetBinder(this);

        isBound = false;

        bindUITarget?.SetDefaultNoNotify();
    }

    public override void NotifyBoundValOfChange(BindValueType newVal)
    {
        if (!isBound)
        {
            Console.WriteLine("Tried to set bound value...but not bound! Bound value was: " + newVal + " of type " + typeof(BindValueType) + " in " + GetType() + " " + this);
            return;
        }

        bindValueTarget?.Set(newVal, false);
        bindValueTarget?.onBoundUIChange?.Invoke(newVal);
    }

    public override void NotifyBoundUIOfChange(BindValueType newVal)
    {
        if (!isBound)
        {
            Console.WriteLine("Tried to set bound value...but not bound! Bound value was: " + newVal + " of type " + typeof(BindValueType) + " in " + GetType() + " " + this);
            return;
        }

        bindUITarget?.Set(newVal, false);
        bindUITarget?.onBoundValueChange?.Invoke(newVal);
    }

    public override string ToString()
    {
        return base.ToString() + " (" + bindValueTarget + ", " + bindUITarget + ")";
    }
}