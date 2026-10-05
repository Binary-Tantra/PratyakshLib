namespace Pratyaksh.Core;

public class PratyakshObject : BaseObject
{
    private Transform transform;

    private readonly List<PratyakshComponent> components;
    private readonly Dictionary<int, int> compIdToIdxMap;

    public Transform Transform { get => transform; }

    internal PratyakshObject(Transform? transform, Transform? parentTransform) : base()
    {
        components = [];
        compIdToIdxMap = [];

        this.transform = transform ?? new Transform(this);
        AddComponent(this.transform);

        this.transform.SetParent(parentTransform, false);
    }

    public PratyakshObject() : this(null, null) { }

    public PratyakshObject(Transform? parentTransform) : this(null, parentTransform) { }

    internal PratyakshObject(PratyakshComponent[] startingComponents) : this()
    {
        for (int i = 0; i < startingComponents.Length; i++)
        {
            AddComponent(startingComponents[i]);
        }
    }

    internal void AddComponent(PratyakshComponent component)
    {
        int idx = components.Count;

        if (!compIdToIdxMap.TryAdd(component.Id, idx))
        {
            Console.WriteLine($"Error: Tried to add duplicate component to object: {this}, Id: {Id}. Component try add Id: {component.Id}");
            return;
        }
        else components.Add(component);
    }

    public T AddComponent<T>() where T : PratyakshComponent, new()
    {
        T newC = new();
        newC.SetOwner(this);

        return newC;
    }

    public PratyakshComponent GetComponent<T>() where T : PratyakshComponent
    {
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i].GetType() is T)
                return components[i];
        }

        return AddComponent<InvalidComponent>();
    }

    public PratyakshComponent[] GetComponents<T>() where T : PratyakshComponent
    {
        List<PratyakshComponent> founds = [];

        for (int i = 0; i < components.Count; i++)
        {
            if (components[i].GetType() is T)
                founds.Add(components[i]);
        }

        return [.. founds];
    }

    public bool RemoveComponent(int id)
    {
        if (!compIdToIdxMap.TryGetValue(id, out int idx))
        {
            Console.WriteLine($"Error: Tried to remove (not part of) component from object: {this}, Id: {Id}. Component try remove Id: {id}");
            return false;
        }
        
        components.RemoveAt(idx);
        compIdToIdxMap.Remove(id);

        return true;
    }

    public bool RemoveComponent<T>() where T : PratyakshComponent
    {
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i].GetType() is T)
            {
                RemoveComponent(components[i].Id);
                return true;
            }
        }

        return false;
    }

    public override void Delete()
    {
        for (int i = 0; i < components.Count; i++)
        {
            components[i].Delete();
        }

        components.Clear();
        compIdToIdxMap.Clear();
    }
}