using System.Numerics;
using Pratyaksh.Core;
using Raylib_cs;

namespace Pratyaksh.UI;

public abstract class BaseRaylibEngine : Engine
{
    protected BaseRaylibCam camera;

    protected string windowName;

    protected string? defaultFontPath;

    protected bool clearScreen;
    protected Color clearColor;

    protected bool drawFPS = false;

    private Font defaultFont;

    public override float DeltaTime => Raylib.GetFrameTime();

    public virtual BaseRaylibCam Camera { get => camera; }

    public BaseRaylibEngine(int width, int height, string windowName, string? defaultFontPath = null, bool clearScreen = true, Color? clearColor = null, bool drawFPS = false, bool initCamera = true, BaseRaylibCam? camera = null) : base()
    {
        if (initCamera)
        {
            this.camera = camera ?? new DefaultRaylibCam(width, height);
            Init(new InteractionManager(this.camera));
        }

        this.windowName = windowName;
        this.defaultFontPath = defaultFontPath;
        this.clearScreen = clearScreen;
        this.clearColor = clearColor ?? Color.DarkGray;
        this.drawFPS = drawFPS;
    }

    protected override InputContext Input()
    {
        InputContext inputContext = new()
        {
            isLMBCurrentlyHeld = Raylib.IsMouseButtonDown(Raylib_cs.MouseButton.Left),
            isRMBCurrentlyHeld = Raylib.IsMouseButtonDown(Raylib_cs.MouseButton.Right),

            wasLMBPressedOnceThisFrame = Raylib.IsMouseButtonPressed(Raylib_cs.MouseButton.Left),
            wasRMBPressedOnceThisFrame = Raylib.IsMouseButtonPressed(Raylib_cs.MouseButton.Right),
            wasLMBReleasedOnceThisFrame = Raylib.IsMouseButtonReleased(Raylib_cs.MouseButton.Left),
            wasRMBReleasedOnceThisFrame = Raylib.IsMouseButtonReleased(Raylib_cs.MouseButton.Right),

            mouseScreenPosition = Raylib.GetMousePosition(),
            mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera.RaylibCam2D),

            mouseWheel = Raylib.GetMouseWheelMove(),

            isCtrlDown = Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftControl) || Raylib.IsKeyDown(Raylib_cs.KeyboardKey.RightControl),
            isShiftDown = Raylib.IsKeyDown(Raylib_cs.KeyboardKey.LeftShift) || Raylib.IsKeyDown(Raylib_cs.KeyboardKey.RightShift)
        };

        int keycode;
        while ((keycode = Raylib.GetKeyPressed()) != 0)
        {
            Raylib_cs.KeyboardKey rkey = (Raylib_cs.KeyboardKey)keycode;

            Core.KeyboardKey key;

            if (rkey == Raylib_cs.KeyboardKey.Backspace)
                key = Core.KeyboardKey.Backspace;
            else if (rkey == Raylib_cs.KeyboardKey.Minus)
                key = Core.KeyboardKey.Minus;
            else if (rkey == Raylib_cs.KeyboardKey.Comma)
                key = Core.KeyboardKey.Comma;
            else if (rkey == Raylib_cs.KeyboardKey.Escape)
                key = Core.KeyboardKey.Escape;
            else if (rkey == Raylib_cs.KeyboardKey.Space)
                key = Core.KeyboardKey.Space;
            else if (rkey == Raylib_cs.KeyboardKey.Enter)
                key = Core.KeyboardKey.Enter;
            else if (rkey == Raylib_cs.KeyboardKey.Tab)
                key = Core.KeyboardKey.Tab;
            else if (rkey == Raylib_cs.KeyboardKey.CapsLock)
                key = Core.KeyboardKey.CapsLock;
            else if (rkey == Raylib_cs.KeyboardKey.Left)
                key = Core.KeyboardKey.LeftArrow;
            else if (rkey == Raylib_cs.KeyboardKey.Right)
                key = Core.KeyboardKey.RightArrow;
            else if (rkey == Raylib_cs.KeyboardKey.Up)
                key = Core.KeyboardKey.UpArrow;
            else if (rkey == Raylib_cs.KeyboardKey.Down)
                key = Core.KeyboardKey.DownArrow;
            else
                key = (Core.KeyboardKey)keycode;

            inputContext.keyboardKeysDown.Add(key);
        }

        return inputContext;
    }

    protected override void Setup()
    {
        Raylib.SetConfigFlags(ConfigFlags.HighDpiWindow);
        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);

        Raylib.InitWindow((int)camera.GetWidth(), (int)camera.GetHeight(), windowName);
        editorObjects.Add(camera);

        Font defF;
        if (Path.Exists(defaultFontPath) && Path.GetExtension(defaultFontPath) == ".ttf")
            defF = Raylib.LoadFont(defaultFontPath);
        else
            defF = Raylib.GetFontDefault();

        defaultFont = defF;
        LayoutEngine.InitSLEDefaultFont(defF);

        OnSetup();
    }

    protected override void UpdateScreen()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();

        if (camera.GetWidth() != sw || camera.GetHeight() != sh)
        {
            InteractionManager.WorldToScreenTransformer.SetScreenSize(new Vector2(sw, sh));
            OnUpdateScreen(sw, sh);
        }
    }

    protected override void Render()
    {
        Raylib.BeginDrawing();
        {
            if (clearScreen) Raylib.ClearBackground(clearColor);

            RenderWorld();
            RenderUI();
            RenderEditorObjects();
            OnRender();
        }
        Raylib.EndDrawing();
    }

    private void RenderWorld()
    {
        Raylib.BeginMode2D(camera.RaylibCam2D);

        for (int i = 0; i < actors.Count; i++)
            actors[i].Render();

        Raylib.EndMode2D();
    }

    private void RenderUI()
    {
        if (drawFPS)
            Raylib.DrawFPS((int)InteractionManager.WorldToScreenTransformer.GetWidth() - 150, 10);

        for (int i = 0; i < uiElements.Count; i++)
            uiElements[i].Render();
    }

    private void RenderEditorObjects()
    {
        for (int i = 0; i < editorObjects.Count; i++)
            editorObjects[i].Render();
    }

    protected override bool IsCloseRequested()
    {
        return Raylib.WindowShouldClose();
    }

    protected override void Cleanup()
    {
        OnCleanup();
        Raylib_cs.Raylib.UnloadFont(defaultFont);
        Raylib.CloseWindow();
    }

    protected abstract void OnSetup();
    protected virtual void OnUpdateScreen(int sw, int sh) { }
    protected virtual void OnRender() { }
    protected virtual void OnCleanup() { }
}