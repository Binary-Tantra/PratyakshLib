using System.Diagnostics;
using System.Numerics;

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

    public event Action? OnHandleInputComplete;

    public abstract float DeltaTime { get; }
    public List<EditorObject> EditorObjects { get => editorObjects; }
    public List<Actor> Actors { get => actors; }
    public List<UIBase> UIElements { get => uiElements; }

    public Engine()
    {
        instance = this;
        actors = [];
        uiElements = [];
        editorObjects = [];
    }

    public void Init(InteractionManager intMan)
    {
        this.intMan = intMan;
        OnInit();
    }

    public double GetTime()
    {
        return Stopwatch.GetTimestamp() / freq;
    }

    public void Start()
    {
        Setup();
        Run();
        Cleanup();
    }

    protected abstract void Setup();

    protected void Run()
    {
        while (!IsCloseRequested())
        {
            UpdateScreen();

            InputContext ipc = Input();
            ipc = OnInput(ipc);

            intMan.UpdateInputContext(ipc);
            intMan.HandleInput();
            OnHandleInputComplete?.Invoke();

            Update();
            Render();
        }
    }

    protected abstract bool IsCloseRequested();

    protected abstract void UpdateScreen();

    protected abstract InputContext Input();

    private void Update()
    {
        if (InteractionManager.WorldToScreenTransformer is EditorObject editorObject)
            editorObject.Update();

        for (int i = 0; i < editorObjects.Count; i++)
            editorObjects[i].Update();

        for (int i = 0; i < actors.Count; i++)
            actors[i].Update();

        for (int i = 0; i < uiElements.Count; i++)
            uiElements[i].Update();

        OnUpdate();
    }

    protected abstract void Render();

    protected abstract void Cleanup();

    protected virtual void OnInit() { }
    
    protected virtual InputContext OnInput(InputContext inputContext)
    {
        return inputContext;
    }

    protected abstract void OnUpdate();
}
