using System.Numerics;
using Raylib_cs;

namespace RaylibNodeLibrary.UI;

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
    private bool isDismissed;

    public string Message { get => message; set => message = value; }
    public AlertType Type { get => alertType; set => alertType = value; }
    public bool IsDismissed => isDismissed;

    public AlertBanner(int posX, int posY, string message, AlertType alertType = AlertType.Error, int width = 360, int height = 32, bool isDismissible = true, int fontSize = 13, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parent, parentBasis)
    {
        selfInteractable = true;

        this.message = message;
        this.alertType = alertType;
        
        this.isDismissible = isDismissible;

        this.fontSize = fontSize;
        
        isDismissed = false;
    }

    protected override void OnDraw()
    {
        if (isDismissed) return;

        (Color accentColor, Color bgColor) = alertType switch
        {
            AlertType.Error => (new Color((byte)218, (byte)54, (byte)51, (byte)255), new Color((byte)60, (byte)20, (byte)20, (byte)220)),
            AlertType.Warning => (new Color((byte)210, (byte)153, (byte)34, (byte)255), new Color((byte)60, (byte)50, (byte)20, (byte)220)),
            AlertType.Success => (new Color((byte)46, (byte)160, (byte)67, (byte)255), new Color((byte)20, (byte)50, (byte)25, (byte)220)),
            AlertType.Info => (new Color((byte)56, (byte)139, (byte)253, (byte)255), new Color((byte)20, (byte)35, (byte)60, (byte)220)),
            _ => (Color.Gray, new Color((byte)40, (byte)40, (byte)40, (byte)220))
        };

        // Draw background box
        Rectangle rect = new Rectangle(Position.X, Position.Y, Width, Height);
        Raylib.DrawRectangleRec(rect, bgColor);
        Raylib.DrawRectangleLinesEx(rect, 1f, accentColor);

        // Draw left accent bar
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y, 4, Height, accentColor);

        // Draw text
        int textX = (int)Position.X + 12;
        int textY = (int)(Position.Y + (Height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(message, textX, textY, Color.White, fontSize, Vector2.Zero);

        // Draw dismiss "X" button
        if (isDismissible)
        {
            int closeX = (int)Position.X + Width - 20;
            int closeY = (int)Position.Y + (Height - fontSize) / 2;
            LayoutEngine.DrawTextAbsolute("x", closeX, closeY, hovered ? Color.White : Color.Gray, fontSize, Vector2.Zero);
        }
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left || isDismissed) return false;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left || isDismissed) return false;

        if (isDismissible)
        {
            float clickX = evt.ScreenPosition.X - Position.X;
            if (clickX >= Width - 25)
            {
                isDismissed = true;
            }
        }

        return true;
    }

    public void Reset()
    {
        isDismissed = false;
    }
}
