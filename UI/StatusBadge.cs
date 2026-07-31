using System.Numerics;
using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public enum StatusType
{
    Active,
    Processing,
    Idle,
    Error,
    Custom
}

public class StatusBadge : UIBase
{
    private string text;
    private StatusType statusType;
    private Color customColor;
    private int fontSize;

    public string Text { get => text; set => text = value; }
    public StatusType Type { get => statusType; set => statusType = value; }
    public Color CustomColor { get => customColor; set => customColor = value; }

    public StatusBadge(int posX, int posY, string text, StatusType statusType = StatusType.Idle, Color? customColor = null, int fontSize = 13, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, 0, 0, parent, parentBasis)
    {
        int w = LayoutEngine.MeasureTextW(text, fontSize) + 24;
        int h = fontSize + 8;

        Size = new Vector2(w, h);

        this.text = text;
        this.statusType = statusType;
        this.customColor = customColor ?? Color.Gray;
        this.fontSize = fontSize;
    }

    protected override void OnDraw()
    {
        Color badgeColor = statusType switch
        {
            StatusType.Active => new Color((byte)46, (byte)160, (byte)67, (byte)255),       // Green
            StatusType.Processing => new Color((byte)210, (byte)153, (byte)34, (byte)255),  // Yellow/Orange
            StatusType.Error => new Color((byte)218, (byte)54, (byte)51, (byte)255),        // Red
            StatusType.Idle => new Color((byte)110, (byte)110, (byte)110, (byte)255),      // Gray
            StatusType.Custom => customColor,
            _ => Color.Gray
        };

        // Draw pill background (semi-transparent)
        Color bgPill = new(badgeColor.R, badgeColor.G, badgeColor.B, (byte)40);
        Rectangle rect = new(Position.X, Position.Y, Width, Height);

        Raylib.DrawRectangleRounded(rect, 0.5f, 6, bgPill);
        Raylib.DrawRectangleRoundedLinesEx(rect, 0.5f, 6, 1f, badgeColor);

        // Draw status dot
        int dotRadius = 4;
        int dotX = (int)Position.X + 8;
        int dotY = (int)Position.Y + Height / 2;
        Raylib.DrawCircle(dotX, dotY, dotRadius, badgeColor);

        // Draw text
        int textX = dotX + 8;
        int textY = (int)(Position.Y + (Height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(text, textX, textY, Color.White, fontSize, Vector2.Zero);
    }
}
