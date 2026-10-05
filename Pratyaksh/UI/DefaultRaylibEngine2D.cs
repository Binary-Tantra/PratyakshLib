namespace Pratyaksh.UI;
using Raylib_cs;

public abstract class DefaultRaylibEngine2D : BaseRaylibEngine2D
{
    public DefaultRaylibEngine2D(int width, int height, string windowName, string? defaultFontPath = null, bool clearScreen = true, Color? clearColor = null, bool drawFPS = false) : base(width, height, windowName, defaultFontPath, clearScreen, clearColor, drawFPS, true) { }
}
