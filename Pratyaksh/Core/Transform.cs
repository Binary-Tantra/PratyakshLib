using System.Numerics;
using System.Runtime.CompilerServices;

namespace Pratyaksh.Core;

public class Transform : PratyakshComponent
{
    protected Transform? parent;

    public Transform? Parent
    {
        get => parent;
        set => SetParent(value, true);
    }

    private Vector3 relativePosition;

    internal Transform(PratyakshObject owner) : base(owner) { }

    public virtual Vector3 RelativePosition { get => relativePosition; set => relativePosition = value; }

    public virtual float RelX { get => relativePosition.X; set => relativePosition.X = value; }
    public virtual float RelY { get => relativePosition.Y; set => relativePosition.Y = value; }
    public virtual float RelZ { get => relativePosition.Z; set => relativePosition.Z = value; }

    public virtual Vector3 Position
    {
        get => Get(Parent?.Position ?? Vector3.Zero, relativePosition);
        set => Set(ref relativePosition, Parent?.Position ?? Vector3.Zero, value);
    }

    public virtual float X
    {
        get => Get(Parent?.X ?? 0, relativePosition.X);
        set => Set(ref relativePosition.X, Parent?.X ?? 0, value);
    }

    public virtual float Y
    {
        get => Get(Parent?.Y ?? 0, relativePosition.Y);
        set => Set(ref relativePosition.Y, Parent?.Y ?? 0, value);
    }

    public virtual float Z
    {
        get => Get(Parent?.Z ?? 0, relativePosition.Z);
        set => Set(ref relativePosition.Z, Parent?.Z ?? 0, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Get<T>(T parentValue, T selfValue) where T : IAdditionOperators<T, T, T>
    {
        return parentValue + selfValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 Get(Vector2 parentValue, Vector2 selfValue)
    {
        return parentValue + selfValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3 Get(Vector3 parentValue, Vector3 selfValue)
    {
        return parentValue + selfValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Set<T>(ref T target, T parentValue, T newValue) where T : ISubtractionOperators<T, T, T>
    {
        T diff = newValue - parentValue;
        target = diff;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Set(ref Vector2 target, Vector2 parentValue, Vector2 newValue)
    {
        Vector2 diff = newValue - parentValue;
        target = diff;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Set(ref Vector3 target, Vector3 parentValue, Vector3 newValue)
    {
        Vector3 diff = newValue - parentValue;
        target = diff;
    }

    public void SetParent(Transform? newParent, bool preservePosition)
    {
        if (newParent == parent)
            return;

        OnSetParent(newParent, parent, preservePosition);
    }

    protected virtual void OnSetParent(Transform? newParent, Transform? oldParent, bool preservePosition)
    {
        Vector3 currAbsPos = Position;
        parent = newParent;

        if (preservePosition) Position = currAbsPos;
        else relativePosition = Vector3.Zero;
    }

    public bool IsAncestor(Transform targetAncestor)
    {
        Transform? curr = this;

        while (curr != null)
        {
            if (curr == targetAncestor) return true;
            curr = curr.Parent;
        }

        return false;
    }

    public static bool IsAncestor(Transform? obj, Transform targetAncestor)
    {
        return obj is Transform tr && tr.IsAncestor(targetAncestor);
    }

    public bool HasAncestorOfType<T>() where T : PratyakshObject
    {
        Transform? curr = this;

        while (curr != null && curr.ownerObject != null)
        {
            if (curr.ownerObject is T) return true;
            curr = curr.Parent;
        }

        return false;
    }

    public static bool HasAncestorOfType<T>(Transform? obj) where T : PratyakshObject
    {
        return obj is Transform tr && tr.HasAncestorOfType<T>();
    }
}