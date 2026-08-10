using System.Diagnostics;

namespace Pratyaksh.Core;

public abstract class Engine
{
    private static Engine instance;
    public static Engine Instance { get => instance; }

    private InteractionManager intMan;
    public InteractionManager InteractionManager { get => intMan; }

    private readonly double freq = Stopwatch.Frequency;

    protected List<EditorObject> editorObjects;
    protected List<Actor> actors;
    protected List<UIBase> uiElements;

    public abstract float DeltaTime { get; }
    public List<EditorObject> EditorObjects { get => editorObjects; }
    public List<Actor> Actors { get => actors; }
    public List<UIBase> UIElements { get => uiElements; }

    public Engine()
    {
        instance = this;
    }

    public void Init(InteractionManager intMan)
    {
        this.intMan = intMan;
    }

    public double GetTime()
    {
        return Stopwatch.GetTimestamp() / freq;
    }

    public void AddActor(Actor actor)
    {
        actors.Add(actor);
    }

    public void RemoveActor(Actor actor)
    {
        actors.Remove(actor);
    }

    public void AddUI(UIBase ui)
    {
        uiElements.Add(ui);
    }

    public void RemoveUI(UIBase ui)
    {
        uiElements.Remove(ui);
    }
}
