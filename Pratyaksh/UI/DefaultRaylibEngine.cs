using System.Numerics;
using Pratyaksh.Core;
using Raylib_cs;

namespace Pratyaksh.UI;

public abstract class DefaultRaylibEngine : Engine
{
    private DefaultRaylibCam camera;

    public override float DeltaTime => Raylib.GetFrameTime();

    public DefaultRaylibCam Camera { get => camera; }

    public DefaultRaylibEngine(int width, int height) : base()
    {
        camera = new DefaultRaylibCam(width, height);
        Init(new InteractionManager(camera));
        OnInit();
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
        Raylib.InitWindow((int)camera.GetWidth(), (int)camera.GetHeight(), "Test Project");
        camera.Setup();

        editorObjects.Add(camera);

        OnSetup();
    }

    protected override void UpdateScreen()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();

        if (camera.GetWidth() != sw || camera.GetHeight() != sh)
        {
            InteractionManager.WorldToScreenTransformer.SetScreenSize(new Vector2(sw, sh));
        }
    }

    protected override void Render()
    {
        OnUpdate();

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.DarkGray);

        OnRender();

        Raylib.EndDrawing();
    }

    protected override bool IsCloseRequested()
    {
        return Raylib.WindowShouldClose();
    }

    protected override void Cleanup()
    {
        OnCleanup();
        Raylib.CloseWindow();
    }

    protected virtual void OnInit() { }
    protected abstract void OnSetup();
    protected abstract void OnUpdate();
    protected abstract void OnRender();
    protected virtual void OnCleanup() { }
}