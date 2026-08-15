namespace Pratyaksh.UI;
using Raylib_cs;

public abstract class DefaultRaylibEngine : BaseRaylibEngine
{
    public DefaultRaylibEngine(int width, int height, string windowName, string? defaultFontPath = null, bool clearScreen = true, Color? clearColor = null, bool drawFPS = false) : base(width, height, windowName, defaultFontPath, clearScreen, clearColor, drawFPS, true) { }
}
