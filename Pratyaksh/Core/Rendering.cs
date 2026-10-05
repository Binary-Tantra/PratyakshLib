namespace Pratyaksh.Core;

public class Rendering : PratyakshComponent
{
    private bool shouldDraw = true;

    public bool ShouldDraw { get => shouldDraw; }

    public Rendering() { }

    public void Show()
    {
        shouldDraw = true;
    }

    public void Hide()
    {
        shouldDraw = false;
    }

    public void Toggle()
    {
        shouldDraw = !shouldDraw;
    }

    public void Render()
    {
        if (shouldDraw) OnRender();
    }

    protected virtual void OnRender() { }
}
