using Raylib_cs;
using LibLayoutEngine;
using System.Numerics;

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

    public StatusBadge(string text, StatusType statusType = StatusType.Idle, Color? customColor = null, int fontSize = 13, Drawable? parent = null) : base(parent)
    {
        this.text = text;
        this.statusType = statusType;
        this.customColor = customColor ?? Color.Gray;
        this.fontSize = fontSize;
    }

    protected override Rectangle OnGetInteractionRect()
    {
        int textW = LayoutEngine.MeasureTextW(text, fontSize);
        return new Rectangle(Position.X, Position.Y, textW + 24, fontSize + 8);
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

        int textW = LayoutEngine.MeasureTextW(text, fontSize);
        int badgeWidth = textW + 24;
        int badgeHeight = fontSize + 8;

        // Draw pill background (semi-transparent)
        Color bgPill = new Color(badgeColor.R, badgeColor.G, badgeColor.B, (byte)40);
        Rectangle rect = new Rectangle(Position.X, Position.Y, badgeWidth, badgeHeight);
        Raylib.DrawRectangleRounded(rect, 0.5f, 6, bgPill);
        Raylib.DrawRectangleRoundedLinesEx(rect, 0.5f, 6, 1f, badgeColor);

        // Draw status dot
        int dotRadius = 4;
        int dotX = (int)Position.X + 8;
        int dotY = (int)Position.Y + badgeHeight / 2;
        Raylib.DrawCircle(dotX, dotY, dotRadius, badgeColor);

        // Draw text
        int textX = dotX + 8;
        int textY = (int)(Position.Y + (badgeHeight - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(text, textX, textY, Color.White, fontSize, Vector2.Zero);
    }
}
