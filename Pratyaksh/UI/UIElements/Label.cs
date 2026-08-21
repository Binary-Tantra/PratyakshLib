using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

public class Label : UIBase
{
    private string text;
    private Raylib_cs.Color textColor;

    public string Text { get => text; set => text = value; }
    public Raylib_cs.Color TextColor { get => textColor; set => textColor = value; }

    public Label(int relativePosX, int relativePosY, string text, int fontSize = 15, Raylib_cs.Color? textColor = null, Drawable? parent = null, ParentBasis? parentBasis = null) : base(relativePosX, relativePosY, 0, 0, parent, parentBasis)
    {
        this.text = text;
        Vector2 textSize = LayoutEngine.MeasureText(text, fontSize);

        int w = (int)textSize.X;
        int h = (int)textSize.Y;

        Size = new Vector2(w, h);

        this.textColor = textColor ?? Raylib_cs.Color.DarkGray;
    }

    protected override void OnDraw()
    {
        LayoutEngine.DrawTextAbsolute(text, (int)Position.X, (int)Position.Y, textColor, Vector2.Zero);
    }
}
