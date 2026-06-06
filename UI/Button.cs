using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class Button : UIBase, IPointerInteractable
{
    private System.Numerics.Vector2 dimensions;
    private string buttonText;

    private Color buttonColor;
    private Color textColor;

    private Color hoverColor;
    
    private int fontSize;
    private bool hasBorder;

    private object payload;

    private Action<Button> onButtonPressed;

    public string ButtonText { get => buttonText; }
    public object Payload { get => payload; }

    public float Width { get => dimensions.X; }
    public float Height { get => dimensions.Y; }

    public Button(float width, float height, string buttonText, Action<Button> onButtonPressed, object payload, int fontSize = 15, bool hasBorder = true, Color? buttonColor = null, Color? textColor = null, Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;

        dimensions.X = Raylib.MeasureText(buttonText, fontSize);
        if (width > dimensions.X) dimensions.X = width;

        dimensions.Y = fontSize + 5;
        if (height > dimensions.Y) dimensions.Y = height;

        this.onButtonPressed = onButtonPressed;
        this.payload = payload;

        this.buttonText = buttonText;
        this.buttonColor = buttonColor ?? Color.LightGray;
        this.textColor = textColor ?? Color.Black;
        this.fontSize = fontSize;
        this.hasBorder = hasBorder;

        if (buttonColor == null)
            hoverColor = Color.RayWhite;
        else hoverColor = new Color(Raymath.Clamp(buttonColor.Value.R + 20, 0, 255), Raymath.Clamp(buttonColor.Value.G + 20, 0, 255), Raymath.Clamp(buttonColor.Value.B + 20, 0, 255), buttonColor.Value.A);
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, dimensions.X, dimensions.Y);
    }

    protected override void OnDraw()
    {
        if (hovered) Raylib.DrawRectangle((int)Position.X, (int)Position.Y, (int)dimensions.X, (int)dimensions.Y, hoverColor);
        else Raylib.DrawRectangle((int)Position.X, (int)Position.Y, (int)dimensions.X, (int)dimensions.Y, buttonColor);

        if (hasBorder) Raylib.DrawRectangleLines((int)Position.X, (int)Position.Y, (int)dimensions.X, (int)dimensions.Y, Color.RayWhite);
        Raylib.DrawText(buttonText, (int)Position.X, (int)Position.Y + fontSize / 2, fontSize - 1, textColor);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        onButtonPressed?.Invoke(this);
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        return false;
    }
}
