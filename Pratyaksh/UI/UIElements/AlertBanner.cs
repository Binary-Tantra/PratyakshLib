using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

public enum AlertType
{
    Info,
    Success,
    Warning,
    Error
}

public class AlertBanner : UIBase, IPointerInteractable
{
    private string message;
    private AlertType alertType;
    private int fontSize;
    private bool isDismissible;

    public event Action<AlertBanner>? OnDismiss;

    public string Message { get => message; set => message = value; }
    public AlertType Type { get => alertType; set => alertType = value; }

    public AlertBanner(int posX, int posY, string message, AlertType alertType = AlertType.Error, int width = 360, int height = 32, bool isDismissible = true, int fontSize = 13, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parent, parentBasis)
    {
        selfInteractable = true;

        this.message = message;
        this.alertType = alertType;
        
        this.isDismissible = isDismissible;

        this.fontSize = fontSize;
    }

    protected override void OnDraw()
    {
        (Raylib_cs.Color accentColor, Raylib_cs.Color bgColor) = alertType switch
        {
            AlertType.Error     => (new Raylib_cs.Color((byte)218, (byte)54, (byte)51, (byte)255), new Raylib_cs.Color((byte)60, (byte)20, (byte)20, (byte)220)),
            AlertType.Warning   => (new Raylib_cs.Color((byte)210, (byte)153, (byte)34, (byte)255), new Raylib_cs.Color((byte)60, (byte)50, (byte)20, (byte)220)),
            AlertType.Success   => (new Raylib_cs.Color((byte)46, (byte)160, (byte)67, (byte)255), new Raylib_cs.Color((byte)20, (byte)50, (byte)25, (byte)220)),
            AlertType.Info      => (new Raylib_cs.Color((byte)56, (byte)139, (byte)253, (byte)255), new Raylib_cs.Color((byte)20, (byte)35, (byte)60, (byte)220)),
            _                   => (Raylib_cs.Color.Gray, new Raylib_cs.Color((byte)40, (byte)40, (byte)40, (byte)220))
        };

        // Draw background box
        Raylib_cs.Rectangle rect = new(Position.X, Position.Y, Width, Height);
        Raylib_cs.Raylib.DrawRectangleRec(rect, bgColor);
        Raylib_cs.Raylib.DrawRectangleLinesEx(rect, 1f, accentColor);

        // Draw left accent bar
        Raylib_cs.Raylib.DrawRectangle((int)Position.X, (int)Position.Y, 4, Height, accentColor);

        // Draw text
        int textX = (int)Position.X + 12;
        int textY = (int)(Position.Y + (Height - fontSize) / 2f);

        LayoutEngine.DrawTextAbsolute(message, textX, textY, Raylib_cs.Color.White, fontSize, Vector2.Zero);

        // Draw dismiss "X" button
        if (isDismissible)
        {
            int closeX = (int)Position.X + Width - 20;
            int closeY = (int)Position.Y + (Height - fontSize) / 2;

            LayoutEngine.DrawTextAbsolute("x", closeX, closeY, hovered ? Raylib_cs.Color.White : Raylib_cs.Color.Gray, fontSize, Vector2.Zero);
        }
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left) return false;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left) return false;

        if (isDismissible)
        {
            float clickX = evt.ScreenPosition.X - Position.X;
            if (clickX >= Width - 25) OnDismiss?.Invoke(this);
        }

        return true;
    }
}
