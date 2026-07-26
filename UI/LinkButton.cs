using Raylib_cs;
using LibLayoutEngine;
using System.Numerics;
using System.Diagnostics;

namespace RaylibNodeLibrary.UI;

public class LinkButton : UIBase, IPointerInteractable
{
    private string text;
    private string url;
    private int fontSize;
    private Action<LinkButton>? onClick;

    public string Text { get => text; set => text = value; }
    public string Url { get => url; set => url = value; }

    public LinkButton(string text, string url, Action<LinkButton>? onClick = null, int fontSize = 14, Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;
        this.text = text;
        this.url = url;
        this.onClick = onClick;
        this.fontSize = fontSize;
    }

    protected override Rectangle OnGetInteractionRect()
    {
        int textW = LayoutEngine.MeasureTextW(text, fontSize);
        return new Rectangle(Position.X, Position.Y, textW, fontSize + 4);
    }

    protected override void OnDraw()
    {
        Color normalColor = new((byte)80, (byte)160, (byte)240, (byte)255);
        Color hoverColor = new((byte)120, (byte)190, (byte)255, (byte)255);
        Color drawColor = hovered ? hoverColor : normalColor;

        int textW = LayoutEngine.MeasureTextW(text, fontSize);
        LayoutEngine.DrawTextAbsolute(text, (int)Position.X, (int)Position.Y, drawColor, fontSize, Vector2.Zero);

        // Draw Underline
        int lineY = (int)Position.Y + fontSize + 1;
        Raylib.DrawLine((int)Position.X, lineY, (int)Position.X + textW, lineY, drawColor);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left) return false;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left) return false;

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
            catch { /* Process start failed */ }
        }

        return true;
    }
}
