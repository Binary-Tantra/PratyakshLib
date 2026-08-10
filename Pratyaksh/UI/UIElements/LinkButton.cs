using System.Numerics;
using System.Diagnostics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

public class LinkButton : UIBase, IPointerInteractable
{
    private string text;
    private string url;
    private int fontSize;
    private Action<LinkButton>? onClick;

    public string Text { get => text; set => text = value; }
    public string Url { get => url; set => url = value; }

    public LinkButton(int posX, int posY, string text, string url, Action<LinkButton>? onClick = null, int fontSize = 14, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, LayoutEngine.MeasureTextW(text, fontSize), LayoutEngine.MeasureTextH(text, fontSize), parent, parentBasis)
    {
        selfInteractable = true;

        this.text = text;
        this.url = url;
        this.onClick = onClick;
        this.fontSize = fontSize;
    }

    protected override void OnDraw()
    {
        Raylib_cs.Color normalColor = new((byte)80, (byte)160, (byte)240, (byte)255);
        Raylib_cs.Color hoverColor = new((byte)120, (byte)190, (byte)255, (byte)255);
        Raylib_cs.Color drawColor = hovered ? hoverColor : normalColor;

        LayoutEngine.DrawTextAbsolute(text, (int)Position.X, (int)Position.Y, drawColor, fontSize, Vector2.Zero);

        // Draw Underline
        int lineY = (int)Position.Y + fontSize + 1;
        Raylib_cs.Raylib.DrawLine((int)Position.X, lineY, (int)Position.X + Width, lineY, drawColor);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        if (onClick != null)
        {
            onClick.Invoke(this);
        }
        else if (!string.IsNullOrWhiteSpace(url))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while trying to open URL: {ex.Message}");
            }
        }

        return true;
    }
}
